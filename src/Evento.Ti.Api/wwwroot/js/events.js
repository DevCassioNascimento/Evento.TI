// ==================================================================
// Sprint 3 – Front Eventos (Cadastro + Lista)
// Listagem + Criação
// Usa SESSION.authFetch() para injetar Bearer Token corretamente.
// ==================================================================

(function () {
  const tbody = document.getElementById("tbodyEvents");
  const btnRefresh = document.getElementById("btnRefresh");
  const btnLogout = document.getElementById("btnLogout");
  const formCreate = document.getElementById("formCreateEvent");

  const errorBox = document.getElementById("errorBox");
  const okBox = document.getElementById("okBox");

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

  function pick(obj, camel, pascal, fallback = "") {
    if (obj && obj[camel] !== undefined && obj[camel] !== null) return obj[camel];
    if (obj && obj[pascal] !== undefined && obj[pascal] !== null) return obj[pascal];
    return fallback;
  }

  function toIsoFromDatetimeLocal(value) {
    // value ex: "2025-12-22T19:30"
    const d = new Date(value);
    if (isNaN(d.getTime())) return "";
    return d.toISOString(); // envia em UTC (backend recebe timestamp with time zone)
  }

  function formatDate(value) {
    if (!value) return "";
    try {
      const d = new Date(value);
      return d.toLocaleString("pt-BR");
    } catch {
      return String(value);
    }
  }

  async function loadEvents() {
    setMsg(null, null);

    if (!ensureLoggedIn()) return;

    const res = await SESSION.authFetch("/api/events", { method: "GET" });

    if (!res.ok) {
      const txt = await res.text();
      setMsg("error", `Falha ao listar eventos: ${res.status} - ${txt}`);
      return;
    }

    const data = await res.json();
    if (tbody) tbody.innerHTML = "";

    if (!Array.isArray(data) || data.length === 0) {
      const tr = document.createElement("tr");
      const td = document.createElement("td");
      td.colSpan = 5;
      td.innerHTML = "<small>Nenhum evento cadastrado.</small>";
      tr.appendChild(td);
      tbody.appendChild(tr);
      return;
    }

    for (const e of data) {
      const id = pick(e, "id", "Id", "");
      const titulo = pick(e, "titulo", "Titulo", "");
      const descricao = pick(e, "descricao", "Descricao", "");
      const dataEvt = pick(e, "data", "Data", "");
      const local = pick(e, "local", "Local", "");
      const depto = pick(e, "departamentoResponsavel", "DepartamentoResponsavel", "");

      const tr = document.createElement("tr");

      const tdTitulo = document.createElement("td");
      tdTitulo.textContent = titulo;
      tr.appendChild(tdTitulo);

      const tdData = document.createElement("td");
      tdData.innerHTML = `<small>${formatDate(dataEvt)}</small>`;
      tr.appendChild(tdData);

      const tdLocal = document.createElement("td");
      tdLocal.innerHTML = `<small>${local || "-"}</small>`;
      tr.appendChild(tdLocal);

      const tdDepto = document.createElement("td");
      tdDepto.innerHTML = `<small>${depto || "-"}</small>`;
      tr.appendChild(tdDepto);

      const tdId = document.createElement("td");
      tdId.innerHTML = `<small>${id}</small>`;
      tr.appendChild(tdId);

      tbody.appendChild(tr);
    }
  }

  async function createEvent(payload) {
    setMsg(null, null);

    if (!ensureLoggedIn()) return;

    const res = await SESSION.authFetch("/api/events", {
      method: "POST",
      body: JSON.stringify(payload)
    });

    if (!res.ok) {
      const txt = await res.text();
      setMsg("error", `Falha ao criar evento: ${res.status} - ${txt}`);
      return;
    }

    let created = null;
    try { created = await res.json(); } catch { /* ignore */ }

    const id = created?.id || created?.Id;
    setMsg("ok", id ? `Evento criado com sucesso. ID: ${id}` : "Evento criado com sucesso.");
    await loadEvents();
  }

  // ------------------------------------------------------------
  // Eventos UI
  // ------------------------------------------------------------
  if (btnRefresh) btnRefresh.addEventListener("click", loadEvents);

  if (btnLogout) {
    btnLogout.addEventListener("click", () => {
      if (typeof SESSION !== "undefined") SESSION.clear();
      redirectToLogin();
    });
  }

  if (formCreate) {
    formCreate.addEventListener("submit", async (ev) => {
      ev.preventDefault();

      const titulo = (document.getElementById("titulo")?.value || "").trim();
      const descricao = (document.getElementById("descricao")?.value || "").trim();
      const dataLocal = (document.getElementById("data")?.value || "").trim();
      const local = (document.getElementById("local")?.value || "").trim();
      const departamentoResponsavel = (document.getElementById("departamentoResponsavel")?.value || "").trim();

      if (!titulo) { setMsg("error", "Título é obrigatório."); return; }
      if (!dataLocal) { setMsg("error", "Data e horário é obrigatório."); return; }
      if (!departamentoResponsavel) { setMsg("error", "Departamento responsável é obrigatório."); return; }

      const dataIso = toIsoFromDatetimeLocal(dataLocal);
      if (!dataIso) { setMsg("error", "Data inválida."); return; }

      const payload = {
        titulo,
        descricao: descricao || null,
        data: dataIso,
        local: local || null,
        departamentoResponsavel
      };

      await createEvent(payload);
      formCreate.reset();
    });
  }

  // Carrega automaticamente
  loadEvents();
})();
