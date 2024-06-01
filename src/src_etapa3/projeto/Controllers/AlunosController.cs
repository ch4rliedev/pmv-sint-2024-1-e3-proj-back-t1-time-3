using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projeto.Data;
using projeto.Models;

namespace projeto.Controllers
{
    public class AlunosController : Controller
    {
        private readonly projetoContext _context;
        private readonly IEmailSender _emailSender;

        public AlunosController(projetoContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender; 
        }

        // GET: Alunos
        public async Task<IActionResult> Index()
        {
            return View(await _context.Aluno.ToListAsync());
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        
        public IActionResult Login()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Aluno aluno)
        {
            var dados = await _context.Aluno.FirstOrDefaultAsync(a => a.Email == aluno.Email);

            if (dados == null || !BCrypt.Net.BCrypt.Verify(aluno.Senha, dados.Senha))
            {
                ViewBag.Message = "Usu�rio ou senha inv�lidos";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, dados.Name),
                new Claim(ClaimTypes.NameIdentifier, dados.Id.ToString()),
                new Claim(ClaimTypes.Email, dados.Email)
            };

            var alunoIdentity = new ClaimsIdentity(claims, "login");
            ClaimsPrincipal principal = new ClaimsPrincipal(alunoIdentity);

            var props = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTime.UtcNow.ToLocalTime().AddHours(8),
                IsPersistent = true
            };

            await HttpContext.SignInAsync(principal, props);

            return Redirect("/");
        }

        public async Task<IActionResult> Logout() 
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login", "Alunos");
        }

        // GET: Alunos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aluno = await _context.Aluno
                .FirstOrDefaultAsync(m => m.Id == id);
            if (aluno == null)
            {
                return NotFound();
            }

            return View(aluno);
        }

        // GET: Alunos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Alunos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Email,Senha")] Aluno aluno)
        {
            if (ModelState.IsValid)
            {
                aluno.Senha = BCrypt.Net.BCrypt.HashPassword(aluno.Senha);
                _context.Add(aluno);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(aluno);
        }

        // GET: Alunos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aluno = await _context.Aluno.FindAsync(id);
            if (aluno == null)
            {
                return NotFound();
            }
            return View(aluno);
        }

        // POST: Alunos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,Senha")] Aluno aluno)
        {
            if (id != aluno.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    
                aluno.Senha = BCrypt.Net.BCrypt.HashPassword(aluno.Senha);
                    _context.Update(aluno);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlunoExists(aluno.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(aluno);
        }

        // GET: Alunos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aluno = await _context.Aluno
                .FirstOrDefaultAsync(m => m.Id == id);
            if (aluno == null)
            {
                return NotFound();
            }

            return View(aluno);
        }

        // POST: Alunos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var aluno = await _context.Aluno.FindAsync(id);
            if (aluno != null)
            {
                _context.Aluno.Remove(aluno);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AlunoExists(int id)
        {
            return _context.Aluno.Any(e => e.Id == id);
        }

        // --- L�gica de recupera��o de senha ---

        //GET
        public IActionResult RecuperarSenha()
        {
            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecuperarSenha(string email, string novaSenha, string confirmarNovaSenha)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Message = "Por favor, insira um endere�o de e-mail.";
                return View();
            }

           
            var aluno = await _context.Aluno.FirstOrDefaultAsync(a => a.Email == email);
            if (aluno == null)
            {
                ViewBag.Message = $"O e-mail '{email}' n�o existe no banco de dados.";
                return View();
            }

  
            if (novaSenha != confirmarNovaSenha)
            {
                ViewBag.Message = "A nova senha e a confirma��o n�o coincidem.";
                return View();
            }

            // Atualizar a senha no banco de dados
            aluno.Senha = BCrypt.Net.BCrypt.HashPassword(novaSenha);
            _context.Update(aluno);
            await _context.SaveChangesAsync();

            ViewBag.Message = $"Redefini��o de senha bem-sucedida para '{email}'.";

            return View();
        }

        // GET
        [HttpGet]
        public IActionResult ConfirmarEmail()
        {
            return View();
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Message = "Por favor, insira um endere�o de e-mail.";
                return View();
            }


            var aluno = await _context.Aluno.FirstOrDefaultAsync(a => a.Email == email);
            if (aluno == null)
            {
                ViewBag.Message = $"O e-mail '{email}' n�o existe no banco de dados.";
                return View();
            }

            // Enviar e-mail com o link de redefini��o de senha (� NECESS�RIO INSERIR UM EMAIL REAL PARA TESTE)
            try
            {

                string resetUrl = Url.Action("RecuperarSenha", "Alunos", new { email = email }, Request.Scheme);


                string subject = "Recupera��o de Senha";
                string message = $"Clique no link para redefinir sua senha: {resetUrl}";

                await _emailSender.SendEmailAsync(email, subject, message);

                ViewBag.Message = $"Um e-mail foi enviado para '{email}'.";
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Erro ao enviar o e-mail: {ex.Message}";
            }

            return View();
        }

    }
}
