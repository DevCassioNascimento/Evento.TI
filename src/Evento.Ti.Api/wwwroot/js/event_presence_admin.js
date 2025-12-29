// ==================================================================
// Sprint 5 – Admin/Staff Presenças (Painel por evento)
// - GET /api/events/{id}
// - GET /api/events/{id}/presences   (RequireRole Admin/Staff)
// ==================================================================

(function () {
  const btnBack = document.getElementById("btnBack");
  const btnLogout = document.getElementById("btnLogout");

  const errorBox = document.getElementById("errorBox");
  const okBox = document.getElementById("okBox");

  const evTitle = document.getElementById("evTitle");
  const evDate = document.getElementById("evDate");
  const evLocal = document.getElementById("evLocal");
  const evDept = document.getElementById("evDept");

  const tbody = document.getElementById("tbodyPresences");
  const summary = document.getElementById("summary");

  const statusFilter = document.getElementById("statusFilter");
  const search = document.getElementById("search");
  const btnReload = document.getElementById("btnReload");

  let cachedItems = [];

  function setMsg(type, text) {
    if (errorBox) { errorBox.style.display = "none"; errorBox.textContent = ""; }
    if (okBox) { okBox.style.display = "none"; okBox.textContent = ""; }
    if (!text) return;

    if (type === "error") {
      errorBox.textContent = text;
      errorBox.style.display = "block";
    } else {
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

  async function loadPresences(eventId) {
    const res = await SESSION.authFetch(`/api/events/${eventId}/presences`, { method: "GET" });

    if (res.status === 403) {
      throw new Error("Acesso negado. Esta tela é apenas para Admin/Staff.");
    }

    if (!res.ok) {
      const txt = await res.text().catch(() => "");
      throw new Error(`Falha ao carregar presenças (${res.status}). ${txt}`);
    }

    return res.json();
  }

  function computeSummary(items) {
    const total = items.length;
    const c = items.filter(x => String(x.status).toLowerCase() === "confirmed").length;
    const d = items.filter(x => String(x.status).toLowerCase() === "declined").length;
    const l = items.filter(x => String(x.status).toLowerCase() === "late").length;
    return { total, c, d, l };
  }

  function applyFilters(items) {
    const f = (statusFilter?.value || "").trim().toLowerCase();
    const q = (search?.value || "").trim().toLowerCase();

    return items.filter(x => {
      const st = String(x.status || "").toLowerCase();
      const name = String(x.userName || "").toLowerCase();
      const email = String(x.userEmail || "").toLowerCase();

      const okStatus = !f || st === f;
      const okQuery = !q || name.includes(q) || email.includes(q);

      return okStatus && okQuery;
    });
  }

  function render(items) {
    if (!tbody) return;

    tbody.innerHTML = "";

    if (!items || items.length === 0) {
      const tr = document.createElement("tr");
      tr.innerHTML = `<td colspan="5">Nenhuma confirmação encontrada.</td>`;
      tbody.appendChild(tr);
      return;
    }

    for (const it of items) {
      const tr = document.createElement("tr");
      tr.innerHTML = `
        <td>${it.userName ?? "—"}</td>
        <td>${it.userEmail ?? "—"}</td>
        <td>${it.status ?? "—"}</td>
        <td>${it.reason ?? ""}</td>
        <td>${formatDate(it.updatedAt)}</td>
      `;
      tbody.appendChild(tr);
    }
  }

  async function refresh(eventId) {
    setMsg("", "");
    tbody.innerHTML = `<tr><td colspan="5">Carregando...</td></tr>`;

    const items = await loadPresences(eventId);
    cachedItems = Array.isArray(items) ? items : [];

    const s = computeSummary(cachedItems);
    if (summary) summary.textContent = `Total: ${s.total} | Confirmados: ${s.c} | Não vão: ${s.d} | Atrasos: ${s.l}`;

    render(applyFilters(cachedItems));
    setMsg("ok", "Painel atualizado.");
  }

  async function init() {
    if (!ensureLoggedIn()) return;

    const eventId = getEventIdFromQuery();
    if (!eventId) {
      setMsg("error", "eventId não informado. Abra a página com ?eventId=GUID");
      return;
    }

    if (btnBack) btnBack.addEventListener("click", () => window.location.href = "/html/events.html");
    if (btnLogout) btnLogout.addEventListener("click", () => { SESSION.clear(); redirectToLogin(); });

    if (btnReload) btnReload.addEventListener("click", () => refresh(eventId));
    if (statusFilter) statusFilter.addEventListener("change", () => render(applyFilters(cachedItems)));
    if (search) search.addEventListener("input", () => render(applyFilters(cachedItems)));

    try {
      const ev = await loadEvent(eventId);
      evTitle.textContent = pick(ev, "titulo", "Titulo", "—");
      evDate.textContent = formatDate(pick(ev, "data", "Data", "—"));
      evLocal.textContent = pick(ev, "local", "Local", "—");
      evDept.textContent = pick(ev, "departamentoResponsavel", "DepartamentoResponsavel", "—");

      await refresh(eventId);
    } catch (e) {
      setMsg("error", e.message || "Erro ao carregar painel.");
    }
  }

  init();
})();
