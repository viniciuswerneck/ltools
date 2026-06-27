# LTools — Plano de Execução

> The Ultimate Desktop Toolkit for Laravel Developers

---

## Estrutura do Projeto

```
D:\www\laravel_tool_kit\
│
├── src/
│   ├── LTools.Core/              (Class Library)
│   │   ├── Models/
│   │   ├── Services/
│   │   ├── Interfaces/
│   │   └── Extensions/
│   └── LTools.UI/                 (Avalonia App)
│       ├── ViewModels/
│       ├── Views/
│       └── Converters/
│
├── plugins/
│   ├── ArtisanGui/          ✅
│   ├── CacheExplorer/       ✅
│   ├── ComposerManager/     ✅
│   ├── Dashboard/           ✅
│   ├── DatabaseDiagram/     ✅
│   ├── DockerManager/       ✅
│   ├── EnvManager/          ✅
│   ├── LogViewer/           ✅
│   ├── ProjectDoctor/       ✅
│   ├── ProjectManager/      ✅
│   ├── QueueMonitor/        ✅
│   ├── RouteExplorer/       ✅
│   ├── Scheduler/           ✅
│   ├── SqlDebugger/         ✅
│   └── VirtualHosts/        ✅
│
├── LTools.sln
├── readme.md
└── PLANO.md
```

---

## Tecnologias

| Tecnologia | Versão |
|-----------|--------|
| C# | 13 |
| .NET | 9.0 |
| Avalonia UI | 11.x |
| Arquitetura | MVVM + DI + Plugins + SOLID |

---

## Fases de Implementação

### Fase 0 — Scaffold ✅

Criação da solution e projetos base.

- ✅ `LTools.sln`
- ✅ `LTools.Core` — Class Library com interfaces e serviços compartilhados
- ✅ `LTools.UI` — Projeto Avalonia App

### Fase 1 — Core (base para todos os módulos) ✅

O Core fornece os serviços fundamentais:

| Serviço | Responsabilidade |
|---------|-----------------|
| `IPluginLoader` | Scan de DLLs em `plugins/`, carrega via reflexão |
| `ILaravelDetector` | Detecta projetos Laravel (artisan, composer.json, .env) |
| `IProcessRunner` | Executa `php artisan` em background com output em tempo real |
| `IConfigManager` | Gerencia configurações do usuário (JSON) |
| `IThemeManager` | Suporte a temas claro/escuro |
| `IMessenger` | Pub/sub interno para comunicação entre módulos |
| `ILaravelProject` | Model representando um projeto Laravel detectado |

### Fases 2 a 16 — Módulos (1 por vez)

#### Módulo 01 — Dashboard ✅
**Pasta:** `plugins/Dashboard/`

Funções:
- Projetos recentes
- PHP instalado (versão)
- Laravel instalado (versão)
- Composer
- Git
- Docker
- Atualizações disponíveis

#### Módulo 02 — Gerenciador de Projetos ✅
**Pasta:** `plugins/ProjectManager/`

Detecta automaticamente: `artisan`, `composer.json`, `.env`, `routes`, `storage`, `database`

Exibe: versão Laravel, versão PHP, banco utilizado, ambiente, tamanho do projeto

#### Módulo 03 — Artisan GUI ✅
**Pasta:** `plugins/ArtisanGui/` ✅

Executa `php artisan list --format=json`.

Funcionalidades:
- Pesquisar comandos
- Executar comandos
- Salvar favoritos
- Histórico
- Parâmetros
- Saída em tempo real

#### Módulo 04 — Env Manager ✅
**Pasta:** `plugins/EnvManager/` ✅

Gerenciamento de ambientes:
- Editar .env com syntax highlighting
- Backup automático ao salvar
- Restaurar backup
- Criar .env a partir de .env.example

#### Módulo 05 — Composer Manager ✅
**Pasta:** `plugins/ComposerManager/` ✅

Interface gráfica para:
- `install`, `update`, `require`, `remove`, `outdated`
- Terminal output em tempo real
- Lista de dependências do composer.json

#### Módulo 06 — Cache Explorer ✅
**Pasta:** `plugins/CacheExplorer/` ✅

Visualização dos caches. Compatível com: File, Redis, Memcached.

Funções:
- Limpar cache da aplicação, config, rotas, views, events
- Limpar tudo de uma vez
- Terminal output em tempo real

#### Módulo 07 — Route Explorer ✅
**Pasta:** `plugins/RouteExplorer/` ✅

Executa `php artisan route:list --json`.

Permite:
- Pesquisar por URI, nome ou action
- Filtrar por middleware
- Exportar para CSV
- Visualizar método HTTP, URI, nome, action, middleware

#### Módulo 08 — Scheduler ✅
**Pasta:** `plugins/Scheduler/` ✅

Lista tarefas agendadas do `php artisan schedule:list`:
- Schedule (cron)
- Comando
- Descrição
- Próxima execução

#### Módulo 09 — Docker Manager ✅
**Pasta:** `plugins/DockerManager/` ✅

Detecta Docker Compose. Permite:
- Iniciar, parar, down, rebuild
- Visualizar logs e status
- Terminal output em tempo real

#### Módulo 10 — Virtual Hosts ✅
**Pasta:** `plugins/VirtualHosts/` ✅

Gerenciamento de Virtual Hosts Apache:
- Criar host com ServerName e DocumentRoot
- Suporte a SSL
- Listar e remover hosts criados

#### Módulo 11 — Log Viewer ✅
**Pasta:** `plugins/LogViewer/` ✅

Monitora `storage/logs/`.

Recursos:
- Listagem de arquivos de log com tamanho e data
- Visualização do conteúdo
- Auto-refresh com FileSystemWatcher
- Suporte a logs grandes (truncamento em 50000 chars)

#### Módulo 12 — Queue Monitor ✅
**Pasta:** `plugins/QueueMonitor/` ✅

Monitoramento completo das filas via Artisan:
- Ver jobs falhos, retentar, limpar
- Executar worker (once), restart
- Monitoramento de filas
- Terminal output em tempo real

#### Módulo 13 — Project Doctor ✅
**Pasta:** `plugins/ProjectDoctor/` ✅

Analisa automaticamente o projeto com score percentual:
- APP_KEY, APP_DEBUG, .env, Storage Link
- Migrations pendentes
- Composer e PHP instalados
- Score de 0-100% com label (Excelente/Bom/Regular/Crítico)

#### Módulo 14 — SQL Debugger ✅
**Pasta:** `plugins/SqlDebugger/` ✅

Visualiza informações do banco de dados:
- Configuração DB do .env (conexão, host, porta, database, usuário)
- Lista de tabelas via `php artisan db:show --json`
- Número de linhas, engine, tamanho

#### Módulo 15 — Database Diagram ✅
**Pasta:** `plugins/DatabaseDiagram/` ✅

Analisa migrations do Laravel para gerar diagrama:
- Lista de tabelas encontradas nas migrations
- Colunas com tipo, nullable, chave primária
- Relacionamentos detectados (foreign keys)
- Visualização detalhada de cada tabela

---

## Padrão de Implementação de cada Plugin

```csharp
public interface ILToolsPlugin
{
    string Name { get; }
    string Icon { get; }
    Task<PluginResult> ExecuteAsync(PluginContext context);
    UserControl GetView();
}
```

Cada plugin é um projeto **Class Library** que gera uma DLL. A UI principal carrega dinamicamente a View + ViewModel.

---

## Ordem de Implementação

| # | Módulo | Dependências | Esforço | Status |
|---|--------|-------------|---------|--------|
| 0 | Scaffold + Core | — | ★ | ✅ |
| 1 | Dashboard | Core | ★ | ✅ |
| 2 | ProjectManager | Core | ★ | ✅ |
| 3 | ArtisanGui | Core + ProcessRunner | ★★ | ✅ |
| 4 | EnvManager | Core | ★ | ✅ |
| 5 | ComposerManager | Core + ProcessRunner | ★★ | ✅ |
| 6 | CacheExplorer | Core + ProcessRunner | ★★ | ✅ |
| 7 | RouteExplorer | Core + ProcessRunner | ★★ | ✅ |
| 8 | Scheduler | Core + ProcessRunner | ★★ | ✅ |
| 9 | DockerManager | Core + ProcessRunner | ★★ | ✅ |
| 10 | VirtualHosts | Core | ★★ | ✅ |
| 11 | LogViewer | Core + FileWatcher | ★★★ | ✅ |
| 12 | QueueMonitor | Core + ProcessRunner | ★★★ | ✅ |
| 13 | ProjectDoctor | Core + vários serviços | ★★★ | ✅ |
| 14 | SqlDebugger | Core + Plugin Laravel | ★★★★ | ✅ |
| 15 | DatabaseDiagram | Core + DB connection | ★★★★★ | ✅ |

---

## Progresso

| Fase | Status |
|------|--------|
| Scaffold + Core | ✅ |
| Dashboard | ✅ |
| ProjectManager | ✅ |
| Artisan GUI | ✅ |
| Env Manager | ✅ |
| Composer Manager | ✅ |
| Cache Explorer | ✅ |
| Route Explorer | ✅ |
| Scheduler | ✅ |
| Docker Manager | ✅ |
| Virtual Hosts | ✅ |
| Log Viewer | ✅ |
| Queue Monitor | ✅ |
| Project Doctor | ✅ |
| SQL Debugger | ✅ |
| Database Diagram | ✅ |

**🎉 Todos os 15 módulos foram implementados!**
