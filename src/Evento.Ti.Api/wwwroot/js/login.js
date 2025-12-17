// Sprint 1 – Autenticação/Autorização (Login)
// Sprint 1 — Controle básico de sessão no front.
// Sprint 2 — Front Inventário: redirecionar para /html/ativos.html após login e salvar Token (T maiúsculo)

const loginForm = document.getElementById("loginForm");
const emailInput = document.getElementById("email");
const passwordInput = document.getElementById("password");
const errorBox = document.getElementById("errorBox");
const submitButton = loginForm ? loginForm.querySelector('button[type="submit"]') : null;

function setError(message) {
  if (!errorBox) return;
  errorBox.style.color = "#fca5a5"; // vermelho
  errorBox.textContent = message || "";
}

function setSuccess(message) {
  if (!errorBox) return;
  errorBox.style.color = "#bbf7d0"; // verde claro
  errorBox.textContent = message || "";
}

async function handleLoginSubmit(event) {
  event.preventDefault();
  setError(""); // limpa erro anterior

  const email = (emailInput?.value || "").trim();
  const password = passwordInput?.value || "";

  if (!email || !password) {
    setError("Informe e-mail e senha.");
    return;
  }

  if (!submitButton) {
    console.error("Botão de submit não encontrado no formulário.");
    return;
  }

  // Desabilita o botão enquanto envia
  submitButton.disabled = true;
  const originalText = submitButton.textContent;
  submitButton.textContent = "Entrando...";

  try {
    const response = await fetch("/api/auth/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email, password })
    });

    if (response.ok) {
      let data = null;
      try {
        data = await response.json();
      } catch {
        // Se não vier JSON, apenas ignora
      }

      // Sprint 1/2 — Controle de sessão: salvar token
      // Importante: sua API retorna "Token" (T maiúsculo)
      const token =
        data?.Token ||          // <- principal (backend atual)
        data?.token ||
        data?.accessToken ||
        data?.jwt ||
        data?.data?.token ||
        data?.data?.Token;

      if (token && typeof SESSION !== "undefined" && SESSION?.setToken) {
        SESSION.setToken(token);
        console.log("Token salvo no sessionStorage (evento_ti_token).");
      } else {
        console.warn("Login OK, mas nenhum token foi encontrado na resposta:", data);
      }

      setSuccess("Login realizado com sucesso.");
      console.log("Resposta do login:", data);

      // Sprint 2 — ir para a tela de Inventário
      window.location.href = "/html/ativos.html";

    } else if (response.status === 401) {
      setError("E-mail ou senha inválidos.");
    } else {
      const text = await response.text();
      console.error("Erro no login:", response.status, text);
      setError("Erro ao realizar login. Código: " + response.status);
    }
  } catch (error) {
    console.error("Falha ao conectar na API:", error);
    setError("Não foi possível conectar ao servidor. Verifique se a API está rodando.");
  } finally {
    submitButton.disabled = false;
    submitButton.textContent = originalText;
  }
}

if (loginForm) {
  loginForm.addEventListener("submit", handleLoginSubmit);
}

console.log("Evento.TI - Tela de login carregada e JS de autenticação inicializado.");
