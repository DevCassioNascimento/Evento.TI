(function () {
  const btnBack = document.getElementById("btnBack");
  const btnLogout = document.getElementById("btnLogout");

  const errorBox = document.getElementById("errorBox");
  const okBox = document.getElementById("okBox");

  const evTitle = document.getElementById("evTitle");
  const evDate = document.getElementById("evDate");
  const evLocal = document.getElementById("evLocal");
  const evDept = document.getElementById("evDept");

  const myStatus = document.getElementById("myStatus");

  const formPresence = document.getElementById("formPresence");
  const reasonEl = document.getElementById("reason");
  const btnReload = document.getElementById("btnReload");

  function setMsg(type, text) {
    if (errorBox) { errorBox.style.display = "none"; errorBox.textContent = ""; }
    if (okBox) { okBox.style.display = "none"; okBox.textContent = ""; }

    if (!text) return;

    if (type === "error") {
      if (!errorBox) return;
      errorBox.textContent = text;
      errorBox.style.display = "block";
    } else {
      if (!okBox) return;
      okBox.textContent = text;
      okBox.style.display = "block";
    }
  }

  function redirectToLogin() {
    window.location.href = "/html/index.html";
  }

  function ensureLoggedIn() {
    if (typeof SESSION === "undefined" || !SESSION.isLoggedIn()) {
      redirectToLogin();
      return false;
    }
    return true;
  }

  function getEventIdFromQuery() {
    const qs = new URLSearchParams(window.location.search);
    return qs.get("eventId") || qs.get("id") || "";
  }

  function pick(obj, camel, pascal, fallback = "") {
    if (obj && obj[camel] !== undefined && obj[camel] !== null) return obj[camel];
    if (obj && obj[pascal] !== undefined && obj[pascal] !== null) return obj[pascal];
    return fallback;
  }

  function formatDate(value) {
    // aceita ISO string ou Date
    try {
      const d = new Date(value);
      if (isNaN(d.getTime())) return String(value ?? "");
      return d.toLocaleString("pt-BR");
    } catch {
      return String(value ?? "");
    }
  }

  async function loadEvent(eventId) {
    const res = await SESSION.authFetch(`/api/events/${eventId}`, { method: "GET" });
    if (!res.ok) {
      const txt = await res.text().catch(() => "");
      throw new Error(`Falha ao carregar evento (${res.status}). ${txt}`);
    }
    return res.json();
  }

  async function loadMyPresence(eventId) {
    const res = await SESSION.authFetch(`/api/events/${eventId}/presence/me`, { method: "GET" });
    if (!res.ok) {
      const txt = await res.text().catch(() => "");
      throw new Error(`Falha ao carregar seu status (${res.status}). ${txt}`);
    }
    return res.json(); // pode vir null
  }

  function setFormFromPresence(presence) {
    // presence pode ser null
    const radios = Array.from(document.querySelectorAll("input[name='status']"));
    radios.forEach(r => (r.checked = false));

    if (!presence) {
      if (myStatus) myStatus.textContent = "Sem confirmação registrada.";
      if (reasonEl) reasonEl.value = "";
      return;
    }

    const status = presence.status || pick(presence, "status", "Status", "");
    const reason = presence.reason || pick(presence, "reason", "Reason", "");

    if (myStatus) {
      myStatus.textContent = `${status}${reason ? " — " + reason : ""}`;
    }

    const radio = radios.find(r => (r.value || "").toLowerCase() === String(status || "").toLowerCase());
    if (radio) radio.checked = true;

    if (reasonEl) reasonEl.value = reason || "";
  }

  async function saveMyPresence(eventId) {
    const selected = document.querySelector("input[name='status']:checked");
    if (!selected) {
      setMsg("error", "Selecione um status (Confirmado / Não vou / Atraso).");
      return;
    }

    const payload = {
      status: selected.value,
      reason: (reasonEl ? reasonEl.value : "") || ""
    };

    const res = await SESSION.authFetch(`/api/events/${eventId}/presence/me`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    });

    if (!res.ok) {
      const txt = await res.text().catch(() => "");
      throw new Error(`Falha ao salvar (${res.status}). ${txt}`);
    }

    return res.json();
  }

  async function init() {
    if (!ensureLoggedIn()) return;

    const eventId = getEventIdFromQuery();
    if (!eventId) {
      setMsg("error", "eventId não informado. Abra a página com ?eventId=GUID");
      return;
    }

    // Botões topo
    if (btnBack) btnBack.addEventListener("click", () => window.location.href = "/html/events.html");
    if (btnLogout) btnLogout.addEventListener("click", () => { SESSION.clear(); redirectToLogin(); });

    if (btnReload) {
      btnReload.addEventListener("click", async () => {
        try {
          setMsg("", "");
          const presence = await loadMyPresence(eventId);
          setFormFromPresence(presence);
          setMsg("ok", "Status recarregado.");
        } catch (e) {
          setMsg("error", e.message || "Erro ao recarregar status.");
        }
      });
    }

    // Carrega dados iniciais
    try {
      setMsg("", "");

      const ev = await loadEvent(eventId);
      evTitle.textContent = pick(ev, "titulo", "Titulo", "—");
      evDate.textContent = formatDate(pick(ev, "data", "Data", "—"));
      evLocal.textContent = pick(ev, "local", "Local", "—");
      evDept.textContent = pick(ev, "departamentoResponsavel", "DepartamentoResponsavel", "—");

      const presence = await loadMyPresence(eventId);
      setFormFromPresence(presence);
    } catch (e) {
      setMsg("error", e.message || "Erro ao carregar dados.");
      return;
    }

    // Submit
    if (formPresence) {
      formPresence.addEventListener("submit", async (ev) => {
        ev.preventDefault();
        try {
          setMsg("", "");
          await saveMyPresence(eventId);
          const presence = await loadMyPresence(eventId);
          setFormFromPresence(presence);
          setMsg("ok", "Presença atualizada com sucesso.");
        } catch (e) {
          setMsg("error", e.message || "Erro ao salvar.");
        }
      });
    }
  }

  init();
})();
