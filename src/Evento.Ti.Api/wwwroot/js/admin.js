// ==================================================================
// Sprint 3 – Eventos (Menu pós-login)
// admin.js — robusto contra variação de IDs e nome do arquivo de Eventos
// ==================================================================

(function () {
  console.log("[admin.js] carregado");

  function redirectToLogin() {
    window.location.href = "/html/index.html";
  }

  // Protege a tela
  if (typeof SESSION === "undefined" || !SESSION.isLoggedIn()) {
    console.warn("[admin.js] SESSION ausente ou não logado. Redirecionando para login.");
    redirectToLogin();
    return;
  }

  // Aceita os dois padrões de IDs (para evitar qualquer divergência)
  const btnAtivos =
    document.getElementById("btnGoAtivos") ||
    document.getElementById("btnAtivos");

  const btnEventos =
    document.getElementById("btnGoEventos") ||
    document.getElementById("btnEventos");

  const btnLogout = document.getElementById("btnLogout");

  if (!btnAtivos) console.warn("[admin.js] Botão Ativos não encontrado (IDs esperados: btnGoAtivos ou btnAtivos).");
  if (!btnEventos) console.warn("[admin.js] Botão Eventos não encontrado (IDs esperados: btnGoEventos ou btnEventos).");

  if (btnAtivos) {
    btnAtivos.addEventListener("click", () => {
      console.log("[admin.js] clicou Ativos");
      window.location.href = "/html/ativos.html";
    });
  }

  async function goToEventos() {
    // Primeiro tenta o nome que criamos: events.html
    // Se não existir, tenta o alternativo: eventos.html
    const candidates = ["/html/events.html", "/html/eventos.html"];

    for (const url of candidates) {
      try {
        const resp = await fetch(url, { method: "HEAD", cache: "no-store" });
        if (resp.ok) {
          console.log("[admin.js] indo para:", url);
          window.location.href = url;
          return;
        }
      } catch {
        // ignora e tenta o próximo
      }
    }

    alert("Não encontrei a tela de Eventos (events.html/eventos.html). Verifique se ela existe em /wwwroot/html/.");
  }

  if (btnEventos) {
    btnEventos.addEventListener("click", () => {
      console.log("[admin.js] clicou Eventos");
      goToEventos();
    });
  }

  if (btnLogout) {
    btnLogout.addEventListener("click", () => {
      console.log("[admin.js] logout");
      SESSION.clear();
      redirectToLogin();
    });
  }
})();
