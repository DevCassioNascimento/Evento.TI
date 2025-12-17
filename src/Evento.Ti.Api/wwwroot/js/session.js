// Sprint 1 — Controle básico de sessão no front.
// Responsabilidade: armazenar token, validar “logado” e padronizar fetch com Authorization.

const SESSION = (() => {
  // Decisão: por padrão usamos sessionStorage (some ao fechar o navegador).
  // Se você preferir manter login após fechar/abrir, troque para localStorage.
  const storage = window.sessionStorage;

  const TOKEN_KEY = "evento_ti_token";

  function setToken(token) {
    if (!token) return;
    storage.setItem(TOKEN_KEY, token);
  }

  function getToken() {
    return storage.getItem(TOKEN_KEY);
  }

  function clear() {
    storage.removeItem(TOKEN_KEY);
  }

  function isLoggedIn() {
    return !!getToken();
  }

  // Wrapper de fetch para APIs protegidas:
  // - injeta Authorization: Bearer <token>
  // - se tomar 401, limpa sessão e redireciona para login
  async function authFetch(url, options = {}) {
    const token = getToken();

    const headers = new Headers(options.headers || {});
    if (token) headers.set("Authorization", `Bearer ${token}`);
    headers.set("Content-Type", headers.get("Content-Type") || "application/json");

    const response = await fetch(url, { ...options, headers });

    if (response.status === 401) {
      clear();
      // Ajuste o destino se seu login for outro arquivo.
      window.location.href = "/html/index.html";
      return response;
    }

    return response;
  }

  return { setToken, getToken, clear, isLoggedIn, authFetch };
})();

// ==================================================================
// Sprint 2 – Front Inventário
// Expor também no window para compatibilidade com scripts que usam window.SESSION
// ==================================================================
window.SESSION = SESSION;
