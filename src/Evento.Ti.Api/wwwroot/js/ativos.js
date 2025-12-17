// ==================================================================
// Sprint 2 – Front Inventário (Ativos)
// Listagem + Criação + Alteração de Status + Exclusão
// Usa SESSION.authFetch() para injetar Bearer Token corretamente.
// ==================================================================

(function () {
  const tbody = document.getElementById("tbodyAtivos");
  const btnRefresh = document.getElementById("btnRefresh");
  const btnLogout = document.getElementById("btnLogout");
  const formCreate = document.getElementById("formCreateAtivo");

  const errorBox = document.getElementById("errorBox");
  const okBox = document.getElementById("okBox");

  function setMsg(type, text) {
    if (errorBox) {
      errorBox.style.display = "none";
      errorBox.textContent = "";
    }
    if (okBox) {
      okBox.style.display = "none";
      okBox.textContent = "";
    }

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

  // ------------------------------------------------------------
  // Sprint 2 – Status enum (int) com rótulos amigáveis
  // IMPORTANTE: se seu enum no back-end tiver outra ordem,
  // ajuste SOMENTE os labels aqui (ou os valores).
  // ------------------------------------------------------------
  const STATUS_OPTIONS = [
    { value: 0, label: "Disponível" },
    { value: 1, label: "Em uso" },
    { value: 2, label: "Manutenção" },
    { value: 3, label: "Emprestado" },
    { value: 4, label: "Indisponível" },
    { value: 5, label: "Baixado" }
  ];

  function statusLabel(value) {
    const found = STATUS_OPTIONS.find(x => x.value === Number(value));
    return found ? found.label : String(value);
  }

  function createStatusSelect(currentValue) {
    const sel = document.createElement("select");
    for (const opt of STATUS_OPTIONS) {
      const o = document.createElement("option");
      o.value = String(opt.value);
      o.textContent = opt.label;
      if (Number(currentValue) === opt.value) o.selected = true;
      sel.appendChild(o);
    }
    return sel;
  }

  function pick(obj, camel, pascal, fallback = "") {
    if (obj && obj[camel] !== undefined && obj[camel] !== null) return obj[camel];
    if (obj && obj[pascal] !== undefined && obj[pascal] !== null) return obj[pascal];
    return fallback;
  }

  async function loadAtivos() {
    setMsg(null, null);

    if (!ensureLoggedIn()) return;

    const res = await SESSION.authFetch("/api/ativos", { method: "GET" });

    if (!res.ok) {
      const txt = await res.text();
      setMsg("error", `Falha ao listar ativos: ${res.status} - ${txt}`);
      return;
    }

    const data = await res.json();
    if (tbody) tbody.innerHTML = "";

    if (!Array.isArray(data) || data.length === 0) {
      const tr = document.createElement("tr");
      const td = document.createElement("td");
      td.colSpan = 5;
      td.innerHTML = "<small>Nenhum ativo cadastrado.</small>";
      tr.appendChild(td);
      tbody.appendChild(tr);
      return;
    }

    for (const a of data) {
      const id = pick(a, "id", "Id", "");
      const name = pick(a, "name", "Name", "");
      const tag = pick(a, "tag", "Tag", "");
      const serial = pick(a, "serialNumber", "SerialNumber", "");
      const status = pick(a, "status", "Status", 0);

      const tr = document.createElement("tr");

      const tdName = document.createElement("td");
      tdName.textContent = name;
      tr.appendChild(tdName);

      const tdTag = document.createElement("td");
      tdTag.innerHTML = `<small>${tag || ""}</small>`;
      tr.appendChild(tdTag);

      const tdSerial = document.createElement("td");
      tdSerial.innerHTML = `<small>${serial || ""}</small>`;
      tr.appendChild(tdSerial);

      const tdStatus = document.createElement("td");
      const sel = createStatusSelect(status);
      tdStatus.appendChild(sel);
      tr.appendChild(tdStatus);

      const tdActions = document.createElement("td");

      const btnSave = document.createElement("button");
      btnSave.type = "button";
      btnSave.className = "primary";
      btnSave.textContent = "Salvar status";
      btnSave.addEventListener("click", async () => {
        await updateStatus(id, Number(sel.value));
      });

      const btnDelete = document.createElement("button");
      btnDelete.type = "button";
      btnDelete.className = "danger";
      btnDelete.style.marginLeft = "8px";
      btnDelete.textContent = "Excluir";
      btnDelete.addEventListener("click", async () => {
        const confirmMsg =
          `Confirmar exclusão do ativo?\n\n` +
          `Nome: ${name}\n` +
          `Tag: ${tag || "-"}\n` +
          `Serial: ${serial || "-"}\n` +
          `Status: ${statusLabel(status)}\n`;

        if (!confirm(confirmMsg)) return;

        await deleteAtivo(id);
      });

      tdActions.appendChild(btnSave);
      tdActions.appendChild(btnDelete);

      tr.appendChild(tdActions);
      tbody.appendChild(tr);
    }
  }

  async function createAtivo(payload) {
    setMsg(null, null);

    if (!ensureLoggedIn()) return;

    const res = await SESSION.authFetch("/api/ativos", {
      method: "POST",
      body: JSON.stringify(payload)
    });

    if (!res.ok) {
      const txt = await res.text();
      setMsg("error", `Falha ao criar ativo: ${res.status} - ${txt}`);
      return;
    }

    setMsg("ok", "Ativo criado com sucesso.");
    await loadAtivos();
  }

  async function updateStatus(id, statusInt) {
    setMsg(null, null);

    if (!id) {
      setMsg("error", "ID do ativo inválido.");
      return;
    }

    if (!ensureLoggedIn()) return;

    const res = await SESSION.authFetch(`/api/ativos/${id}/status`, {
      method: "PUT",
      body: JSON.stringify({ status: statusInt })
    });

    if (!res.ok) {
      const txt = await res.text();
      setMsg("error", `Falha ao alterar status: ${res.status} - ${txt}`);
      return;
    }

    setMsg("ok", "Status atualizado com sucesso.");
    await loadAtivos();
  }

  async function deleteAtivo(id) {
    setMsg(null, null);

    if (!id) {
      setMsg("error", "ID do ativo inválido.");
      return;
    }

    if (!ensureLoggedIn()) return;

    const res = await SESSION.authFetch(`/api/ativos/${id}`, {
      method: "DELETE"
    });

    if (res.status === 204) {
      setMsg("ok", "Ativo excluído com sucesso.");
      await loadAtivos();
      return;
    }

    if (!res.ok) {
      const txt = await res.text();
      setMsg("error", `Falha ao excluir ativo: ${res.status} - ${txt}`);
      return;
    }

    // fallback raro (caso backend retorne 200)
    setMsg("ok", "Ativo excluído com sucesso.");
    await loadAtivos();
  }

  // ------------------------------------------------------------
  // Eventos
  // ------------------------------------------------------------
  if (btnRefresh) btnRefresh.addEventListener("click", loadAtivos);

  if (btnLogout) {
    btnLogout.addEventListener("click", () => {
      if (typeof SESSION !== "undefined") SESSION.clear();
      redirectToLogin();
    });
  }

  if (formCreate) {
    formCreate.addEventListener("submit", async (e) => {
      e.preventDefault();

      const name = (document.getElementById("name")?.value || "").trim();
      const tag = (document.getElementById("tag")?.value || "").trim();
      const serialNumber = (document.getElementById("serialNumber")?.value || "").trim();

      if (!name) {
        setMsg("error", "Nome é obrigatório.");
        return;
      }

      const payload = {
        name,
        tag: tag || null,
        serialNumber: serialNumber || null
        // status não vai no POST (backend define default)
      };

      await createAtivo(payload);
      formCreate.reset();
    });
  }

  // Carrega automaticamente
  loadAtivos();
})();
