// ==================================================================
// Sprint 4 – Front Eventos (Cadastro + Lista + Edição)
// - Mantém criação (POST /api/events)
// - Adiciona edição (PUT /api/events/{id})
// - Renderiza coluna Ações (Editar/Cancelar)
// - Usa SESSION.authFetch() para Bearer Token
// ==================================================================

(function () {
  const tbody = document.getElementById("tbodyEvents");
  const btnRefresh = document.getElementById("btnRefresh");
  const btnLogout = document.getElementById("btnLogout");
  const formCreate = document.getElementById("formCreateEvent");
  const btnCreate = document.getElementById("btnCreate");

  const errorBox = document.getElementById("errorBox");
  const okBox = document.getElementById("okBox");

  // Estado de edição
  let editingId = null;

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
    const d = new Date(value);
    if (isNaN(d.getTime())) return "";
    return d.toISOString();
  }

  function toDatetimeLocalFromIso(iso) {
    if (!iso) return "";
    const d = new Date(iso);
    if (isNaN(d.getTime())) return "";
    const pad = (n) => String(n).padStart(2, "0");
    const yyyy = d.getFullYear();
    const mm = pad(d.getMonth() + 1);
    const dd = pad(d.getDate());
    const hh = pad(d.getHours());
    const mi = pad(d.getMinutes());
    return `${yyyy}-${mm}-${dd}T${hh}:${mi}`;
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

  function clearEditingMode() {
    editingId = null;
    if (btnCreate) btnCreate.textContent = "Criar";
  }

  function setEditingMode(evt) {
    editingId = pick(evt, "id", "Id", null);
    if (btnCreate) btnCreate.textContent = "Salvar";

    // Preenche formulário
    document.getElementById("titulo").value = pick(evt, "titulo", "Titulo", "");
    document.getElementById("descricao").value = pick(evt, "descricao", "Descricao", "");
    document.getElementById("data").value = toDatetimeLocalFromIso(pick(evt, "data", "Data", ""));
    document.getElementById("local").value = pick(evt, "local", "Local", "");
    document.getElementById("departamentoResponsavel").value = pick(evt, "departamentoResponsavel", "DepartamentoResponsavel", "");

    window.scrollTo({ top: 0, behavior: "smooth" });
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
      td.colSpan = 6; // agora existe coluna Ações
      td.innerHTML = "<small>Nenhum evento cadastrado.</small>";
      tr.appendChild(td);
      tbody.appendChild(tr);
      return;
    }

    for (const e of data) {
      const id = pick(e, "id", "Id", "");
      const titulo = pick(e, "titulo", "Titulo", "");
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

      // Sprint 4: Ações (Editar / Cancelar)
      const tdAcoes = document.createElement("td");

      const btnEdit = document.createElement("button");
      btnEdit.type = "button";
      btnEdit.textContent = "Editar";
      btnEdit.className = "secondary";
      btnEdit.addEventListener("click", () => {
        setMsg(null, null);
        setEditingMode(e);
      });

      const btnCancel = document.createElement("button");
      btnCancel.type = "button";
      btnCancel.textContent = "Excluir";
      btnCancel.className = "danger";
      btnCancel.style.marginLeft = "8px";

    // Sprint 5: Presença (Equipe) + Painel (Admin/Staff) - Inicio
    const btnPresence = document.createElement("button");
    btnPresence.type = "button";
    btnPresence.textContent = "Confirmar presença";
    btnPresence.className = "secondary";
    btnPresence.style.marginLeft = "8px";
    btnPresence.addEventListener("click", () => {
      window.location.href = `/html/event_presence.html?eventId=${encodeURIComponent(id)}`;
    });

    const btnPresenceAdmin = document.createElement("button");
    btnPresenceAdmin.type = "button";
    btnPresenceAdmin.textContent = "Ver presenças";
    btnPresenceAdmin.className = "secondary";
    btnPresenceAdmin.style.marginLeft = "8px";
    btnPresenceAdmin.addEventListener("click", () => {
      window.location.href = `/html/event_presence_admin.html?eventId=${encodeURIComponent(id)}`;
    });
    // Sprint 5: Presença (Equipe) + Painel (Admin/Staff) - Fim


      // Sprint 4: força exibição mesmo se o CSS tiver display:none !important
      tdAcoes.style.whiteSpace = "nowrap";
      tdAcoes.style.minWidth = "360px";

      btnEdit.style.setProperty("display", "inline-block", "important");
      btnEdit.style.setProperty("visibility", "visible", "important");
      btnEdit.style.setProperty("opacity", "1", "important");

      btnCancel.style.setProperty("display", "inline-block", "important");
      btnCancel.style.setProperty("visibility", "visible", "important");
      btnCancel.style.setProperty("opacity", "1", "important");

      btnPresence.style.setProperty("display", "inline-block", "important");
      btnPresence.style.setProperty("visibility", "visible", "important");
      btnPresence.style.setProperty("opacity", "1", "important");

      btnPresenceAdmin.style.setProperty("display", "inline-block", "important");
      btnPresenceAdmin.style.setProperty("visibility", "visible", "important");
      btnPresenceAdmin.style.setProperty("opacity", "1", "important");
      // Sprint 4: força exibição mesmo se o CSS tiver display:none !important
 
      
      // excluir evento - funcionalidade extra
      btnCancel.addEventListener("click", async () => {
        setMsg(null, null);

        const idToDelete = pick(e, "id", "Id", "");
        if (!idToDelete) {
          setMsg("error", "ID do evento não encontrado para exclusão.");
          return;
        }

        const ok = confirm("Confirma excluir este evento?");
        if (!ok) return;

        const resDel = await SESSION.authFetch(`/api/events/${idToDelete}`, { method: "DELETE" });

        if (resDel.status === 204) {
          // Se eu estava editando o mesmo evento, limpa o form
          if (editingId === idToDelete) {
            formCreate.reset();
            clearEditingMode();
          }

          setMsg("ok", "Evento excluído com sucesso.");
          await loadEvents();
          return;
        }

        // 409 = conflito (ex.: tem ativos vinculados)
        const txt = await resDel.text();
        setMsg("error", `Falha ao excluir: ${resDel.status} - ${txt}`);
      });


      // excluir evento - funcionalidade extra

      tdAcoes.appendChild(btnEdit);
      tdAcoes.appendChild(btnCancel);
      tdAcoes.appendChild(btnPresence);
      tdAcoes.appendChild(btnPresenceAdmin);
      tr.appendChild(tdAcoes);
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
    try { created = await res.json(); } catch { }

    const id = created?.id || created?.Id;
    setMsg("ok", id ? `Evento criado com sucesso. ID: ${id}` : "Evento criado com sucesso.");
  }

  async function updateEvent(id, payload) {
    setMsg(null, null);

    if (!ensureLoggedIn()) return;

    const res = await SESSION.authFetch(`/api/events/${id}`, {
      method: "PUT",
      body: JSON.stringify(payload)
    });

    if (!res.ok) {
      const txt = await res.text();
      setMsg("error", `Falha ao editar evento: ${res.status} - ${txt}`);
      return false;
    }

    setMsg("ok", "Evento atualizado com sucesso.");
    return true;
  }

  // UI events
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

      // Sprint 4: se estiver editando, salva; senão, cria
      if (editingId) {
        const ok = await updateEvent(editingId, payload);
        if (ok) {
          formCreate.reset();
          clearEditingMode();
          await loadEvents();
        }
        return;
      }

      await createEvent(payload);
      formCreate.reset();
      await loadEvents();
    });
  }

  clearEditingMode();
  loadEvents();
})();
