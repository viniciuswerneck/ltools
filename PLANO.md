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
│   ├── CacheExplorer/
│   ├── ComposerManager/
│   ├── Dashboard/           ✅
│   ├── DatabaseDiagram/
│   ├── DockerManager/
│   ├── EnvManager/          ✅
│   ├── LogViewer/
│   ├── ProjectDoctor/
│   ├── ProjectManager/      ✅
│   ├── QueueMonitor/
│   ├── RouteExplorer/
│   ├── Scheduler/
│   ├── SqlDebugger/
│   └── VirtualHosts/
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

#### Módulo 05 — Composer Manager
**Pasta:** `plugins/ComposerManager/`

Interface gráfica para:
- `install`, `update`, `require`, `remove`, `outdated`

#### Módulo 06 — Cache Explorer
**Pasta:** `plugins/CacheExplorer/`

Visualização dos caches. Compatível com: File, Redis, Memcached.

Funções:
- Pesquisar chave
- Remover chave
- Limpar cache

#### Módulo 07 — Route Explorer
**Pasta:** `plugins/RouteExplorer/`

Executa `php artisan route:list --json`.

Permite:
- Pesquisar
- Filtrar por middleware
- Filtrar por controller
- Exportar documentação
- Visualizar parâmetros

#### Módulo 08 — Scheduler
**Pasta:** `plugins/Scheduler/`

Lista tarefas agendadas:
- `Schedule::command()`
- `Schedule::job()`
- `Schedule::call()`

Mostra: próxima execução, última execução, frequência

#### Módulo 09 — Docker Manager
**Pasta:** `plugins/DockerManager/`

Detecta Docker Compose. Permite:
- Iniciar, parar, rebuild
- Visualizar logs

#### Módulo 10 — Virtual Hosts
**Pasta:** `plugins/VirtualHosts/`

Integração com Virtual Hosts Manager:
- Criar host, editar, remover
- SSL
- Abrir navegador

#### Módulo 11 — Log Viewer
**Pasta:** `plugins/LogViewer/`

Monitora `storage/logs/`.

Recursos:
- Atualização automática (FileWatcher)
- Filtros
- Destaque de exceções
- Copiar stacktrace
- Pesquisar

#### Módulo 12 — Queue Monitor
**Pasta:** `plugins/QueueMonitor/`

Monitoramento completo das filas:
- Jobs em execução
- Jobs falhados
- Tempo médio
- Reiniciar workers
- Limpar filas

#### Módulo 13 — Project Doctor
**Pasta:** `plugins/ProjectDoctor/`

Analisa automaticamente o projeto. Verificações:
- APP_KEY, APP_DEBUG, Storage Link
- Config Cache, Route Cache
- Composer, PHP, Extensões
- Permissões, Queue, Scheduler
- Banco, Cache, Logs, SSL
- Variáveis não utilizadas
- Migrations pendentes
- Packages desatualizados

Gera relatório com score percentual.

#### Módulo 14 — SQL Debugger
**Pasta:** `plugins/SqlDebugger/`

Plugin Laravel (pacote Composer) que transmite queries em tempo real.

Exibe:
- SQL executado
- Tempo de execução
- Bindings
- EXPLAIN
- Queries duplicadas
- Consultas lentas

#### Módulo 15 — Database Diagram
**Pasta:** `plugins/DatabaseDiagram/`

Conecta ao banco e desenha automaticamente:
- Tabelas
- Relacionamentos
- Chaves estrangeiras
- Índices

Exporta: PNG, SVG, PDF

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
| 5 | ComposerManager | Core + ProcessRunner | ★★ | ⏳ |
| 6 | CacheExplorer | Core + ProcessRunner | ★★ | ⏳ |
| 7 | RouteExplorer | Core + ProcessRunner | ★★ | ⏳ |
| 8 | Scheduler | Core + ProcessRunner | ★★ | ⏳ |
| 9 | DockerManager | Core + ProcessRunner | ★★ | ⏳ |
| 10 | VirtualHosts | Core | ★★ | ⏳ |
| 11 | LogViewer | Core + FileWatcher | ★★★ | ⏳ |
| 12 | QueueMonitor | Core + ProcessRunner | ★★★ | ⏳ |
| 13 | ProjectDoctor | Core + vários serviços | ★★★ | ⏳ |
| 14 | SqlDebugger | Core + Plugin Laravel | ★★★★ | ⏳ |
| 15 | DatabaseDiagram | Core + DB connection | ★★★★★ | ⏳ |

---

## Progresso

| Fase | Status |
|------|--------|
| Scaffold + Core | ✅ |
| Dashboard | ✅ |
| ProjectManager | ✅ |
| Artisan GUI | ✅ |
| Env Manager | ✅ |
| Composer Manager | ⏳ Próximo |
| Demais módulos | ⏳ |

**Próximo:** Módulo 05 — Composer Manager.
