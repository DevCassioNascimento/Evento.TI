// Sprint 4 - Tela Checklist do Evento (Ativos por Evento) - JS separado

const API_BASE = ""; // mesma origem (localhost:5268)

const elToken = document.getElementById("token");
const elEventSelect = document.getElementById("eventSelect");
const elAtivoSelect = document.getElementById("ativoSelect");
const elBtnLoad = document.getElementById("btnLoad");
const elBtnAdd = document.getElementById("btnAddAtivo");
const elTbody = document.getElementById("tbody");
const elApiStatus = document.getElementById("apiStatus");
const elMsg = document.getElementById("msg");

function toast(text) {
  elMsg.textContent = text;
  elMsg.style.display = "inline-block";
  setTimeout(() => (elMsg.style.display = "none"), 3500);
}

function setApiStatus(ok, text) {
  elApiStatus.textContent = text;
  elApiStatus.style.background = ok ? "#dcfce7" : "#fee2e2";
  elApiStatus.style.color = ok ? "#166534" : "#991b1b";
  elApiStatus.style.borderRadius = "999px";
  elApiStatus.style.padding = "6px 10px";
  elApiStatus.style.display = "inline-block";
}

function escapeHtml(s) {
  return String(s)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#039;");
}

function looksLikeJwt(v) {
  if (!v) return false;
  const s = String(v).trim().replace(/^Bearer\s+/i, "");
  const parts = s.split(".");
  return parts.length === 3 && parts[0].length > 10 && parts[1].length > 10;
}

function extractTokenFromValue(raw) {
  if (!raw) return "";

  // já vem como "Bearer xxx"
  let s = String(raw).trim();
  s = s.replace(/^Bearer\s+/i, "").trim();

  // se é JWT direto, ok
  if (looksLikeJwt(s)) return s;

  // tenta parsear JSON e achar token em chaves comuns
  try {
    const obj = JSON.parse(s);

    const candidates = [
      obj.token,
      obj.Token,
      obj.accessToken,
      obj.AccessToken,
      obj.jwt,
      obj.JWT,
      obj.authToken,
      obj.AuthToken,
    ].filter(Boolean);

    for (const c of candidates) {
      const t = extractTokenFromValue(c);
      if (t) return t;
    }

    // caso esteja aninhado
    if (obj.data) {
      const t = extractTokenFromValue(obj.data);
      if (t) return t;
    }
    if (obj.user) {
      const t = extractTokenFromValue(obj.user);
      if (t) return t;
    }
  } catch {
    // não é JSON, segue
  }

  return "";
}

function scanStorageForToken(storage) {
  // 1) tenta chaves “clássicas”
  const directKeys = [
    "token", "Token",
    "accessToken", "AccessToken",
    "jwt", "JWT",
    "authToken", "AuthToken",
    "auth", "Auth",
    "session", "Session",
    "user", "User",
    "login", "Login"
  ];

  for (const k of directKeys) {
    const v = storage.getItem(k);
    const t = extractTokenFromValue(v);
    if (t) return t;
  }

  // 2) varre todas as chaves (quando o nome é específico do projeto)
  for (let i = 0; i < storage.length; i++) {
    const key = storage.key(i);
    const v = storage.getItem(key);
    const t = extractTokenFromValue(v);
    if (t) return t;
  }

  return "";
}

function getToken() {
  // 1) se usuário digitou/colou, prioriza
  const typed = (elToken.value || "").trim();
  if (typed) return typed.replace(/^Bearer\s+/i, "").trim();

  // 2) tenta sessionStorage / localStorage com detecção robusta
  const t1 = scanStorageForToken(sessionStorage);
  if (t1) {
    elToken.value = t1;
    return t1;
  }

  const t2 = scanStorageForToken(localStorage);
  if (t2) {
    elToken.value = t2;
    return t2;
  }

  return "";
}

async function apiFetch(path, options = {}) {
  const token = getToken();
  const headers = options.headers || {};
  headers["Content-Type"] = "application/json";
  if (token) headers["Authorization"] = `Bearer ${token}`;

  return fetch(API_BASE + path, { ...options, headers });
}

function renderRows(items) {
  if (!items || items.length === 0) {
    elTbody.innerHTML = `<tr><td colspan="6" class="muted">Nenhum ativo vinculado a este evento ainda.</td></tr>`;
    return;
  }

  elTbody.innerHTML = items
    .map((it) => {
      const checked = it.isSeparado ? "checked" : "";
      return `
        <tr data-ativo-id="${it.ativoId}">
          <td>${escapeHtml(it.name)}</td>
          <td>${escapeHtml(it.tag || "")}</td>
          <td>${escapeHtml(it.serialNumber || "")}</td>
          <td><span class="pill">${escapeHtml(String(it.status))}</span></td>
          <td>
            <input type="checkbox" class="chkSeparado" ${checked} />
          </td>
          <td class="right">
            <button class="danger btnRemove">Remover</button>
          </td>
        </tr>
      `;
    })
    .join("");

  document.querySelectorAll(".chkSeparado").forEach((chk) => {
    chk.addEventListener("change", async (ev) => {
      const tr = ev.target.closest("tr");
      const ativoId = tr.getAttribute("data-ativo-id");
      const eventId = elEventSelect.value;
      const isSeparado = ev.target.checked;

      const resp = await apiFetch(`/api/events/${eventId}/ativos/${ativoId}/separado`, {
        method: "PUT",
        body: JSON.stringify({ isSeparado }),
      });

      if (!resp.ok) {
        ev.target.checked = !isSeparado;
        const txt = await resp.text();
        toast(`Erro ao atualizar: ${resp.status} - ${txt}`);
        setApiStatus(false, "API: erro no update");
        return;
      }

      setApiStatus(true, "API: ok");
    });
  });

  document.querySelectorAll(".btnRemove").forEach((btn) => {
    btn.addEventListener("click", async (ev) => {
      const tr = ev.target.closest("tr");
      const ativoId = tr.getAttribute("data-ativo-id");
      const eventId = elEventSelect.value;

      if (!confirm("Remover este ativo do evento?")) return;

      const resp = await apiFetch(`/api/events/${eventId}/ativos/${ativoId}`, { method: "DELETE" });

      if (!resp.ok) {
        const txt = await resp.text();
        toast(`Erro ao remover: ${resp.status} - ${txt}`);
        setApiStatus(false, "API: erro no delete");
        return;
      }

      toast("Vínculo removido.");
      await loadChecklist();
      setApiStatus(true, "API: ok");
    });
  });
}

async function loadEvents() {
  const resp = await apiFetch("/api/events", { method: "GET" });

  if (resp.status === 401) {
    setApiStatus(false, "API: 401 (token)");
    elEventSelect.innerHTML = `<option value="">(faça login / informe token)</option>`;
    return;
  }

  if (!resp.ok) {
    const txt = await resp.text();
    setApiStatus(false, `API: erro (${resp.status})`);
    elEventSelect.innerHTML = `<option value="">(erro ao carregar eventos)</option>`;
    toast(txt);
    return;
  }

  const items = await resp.json();
  elEventSelect.innerHTML = items
    .map((e) => `<option value="${e.id}">${escapeHtml(e.titulo)} — ${new Date(e.data).toLocaleString()}</option>`)
    .join("");

  setApiStatus(true, "API: ok");
}

async function loadAtivosInventario() {
  const resp = await apiFetch("/api/ativos", { method: "GET" });

  if (resp.status === 401) {
    setApiStatus(false, "API: 401 (token)");
    elAtivoSelect.innerHTML = `<option value="">(faça login / informe token)</option>`;
    return;
  }

  if (!resp.ok) {
    const txt = await resp.text();
    setApiStatus(false, `API: erro (${resp.status})`);
    elAtivoSelect.innerHTML = `<option value="">(erro ao carregar ativos)</option>`;
    toast(txt);
    return;
  }

  const items = await resp.json();
  elAtivoSelect.innerHTML = items
    .map((a) => `<option value="${a.id}">${escapeHtml(a.name)}${a.tag ? " (" + escapeHtml(a.tag) + ")" : ""}</option>`)
    .join("");

  setApiStatus(true, "API: ok");
}

async function loadChecklist() {
  const eventId = elEventSelect.value;

  if (!eventId) {
    elTbody.innerHTML = `<tr><td colspan="6" class="muted">Selecione um evento.</td></tr>`;
    return;
  }

  const resp = await apiFetch(`/api/events/${eventId}/ativos`, { method: "GET" });

  if (resp.status === 401) {
    setApiStatus(false, "API: 401 (token)");
    toast("Não autorizado. Informe o token.");
    return;
  }

  if (resp.status === 404) {
    setApiStatus(false, "API: 404 (evento)");
    toast("Evento não encontrado.");
    return;
  }

  if (!resp.ok) {
    const txt = await resp.text();
    setApiStatus(false, `API: erro (${resp.status})`);
    toast(txt);
    return;
  }

  const items = await resp.json();
  renderRows(items);
  setApiStatus(true, "API: ok");
}

async function reloadAllIfTokenPresent() {
  const t = getToken();
  if (!t) {
    setApiStatus(false, "API: 401 (token)");
    return;
  }
  await loadEvents();
  await loadAtivosInventario();
}

// UI events
elBtnLoad.addEventListener("click", loadChecklist);

elBtnAdd.addEventListener("click", async () => {
  const eventId = elEventSelect.value;
  const ativoId = elAtivoSelect.value;

  if (!eventId) return toast("Selecione um evento.");
  if (!ativoId) return toast("Selecione um ativo.");

  const resp = await apiFetch(`/api/events/${eventId}/ativos/${ativoId}`, { method: "POST" });

  if (resp.status === 401) {
    setApiStatus(false, "API: 401 (token)");
    return toast("Não autorizado. Informe o token.");
  }

  if (resp.status === 409) {
    setApiStatus(false, "API: 409 (já vinculado)");
    return toast("Esse ativo já está vinculado ao evento.");
  }

  if (!resp.ok) {
    const txt = await resp.text();
    setApiStatus(false, `API: erro (${resp.status})`);
    return toast(txt);
  }

  toast("Ativo vinculado ao evento.");
  await loadChecklist();
  setApiStatus(true, "API: ok");
});

// Quando colar/digitar token, recarrega automaticamente
elToken.addEventListener("input", () => {
  // pequena proteção: só tenta recarregar se parece token
  const t = (elToken.value || "").trim();
  if (t.length > 20) reloadAllIfTokenPresent();
});

// init
(async function init() {
  getToken(); // tenta auto-preencher do storage
  await loadEvents();
  await loadAtivosInventario();
})();
