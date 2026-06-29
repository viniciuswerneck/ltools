<div align="center">
  <br/>
  <h1>⚡ LTools</h1>
  <p><strong>The Ultimate Desktop Toolkit for Laravel Developers</strong></p>
  <br/>

  ![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)
  ![C#](https://img.shields.io/badge/C%23-13-239120?style=flat-square&logo=csharp)
  ![Avalonia UI](https://img.shields.io/badge/Avalonia_UI-12.x-8B5CF6?style=flat-square)
  ![License](https://img.shields.io/badge/license-MIT-green?style=flat-square)
  ![Platform](https://img.shields.io/badge/platform-Windows%20|%20Linux%20|%20macOS-blue?style=flat-square)
  ![PRs](https://img.shields.io/badge/PRs-welcome-brightgreen?style=flat-square)

  <br/>

  <img src="docs/screenshot.png" alt="LTools Screenshot" width="800"/>

  <br/>
  <br/>

  <p>
    <a href="#-sobre">Sobre</a> •
    <a href="#-funcionalidades">Funcionalidades</a> •
    <a href="#-plugins">Plugins</a> •
    <a href="#-instalação">Instalação</a> •
    <a href="#-requisitos">Requisitos</a> •
    <a href="#-como-contribuir">Contribuir</a>
  </p>

  <br/>
</div>

---

## 📋 Sobre

**LTools** é uma suíte de ferramentas desktop moderna, gratuita e de código aberto, criada para aumentar a produtividade de desenvolvedores **Laravel**.

Em vez de ficar alternando entre terminal, editor, navegador, gerenciador de banco de dados e monitor de logs, o LTools centraliza **tudo o que você precisa** em uma única aplicação rápida e elegante.

> 🎯 **Missão:** Ser o centro de operações definitivo para projetos Laravel — open source, gratuito e construído pela comunidade.

---

## ✨ Filosofia

| Princípio | Descrição |
|-----------|-----------|
| **🔓 Open Source** | Código 100% aberto. Sem licenças, sem trials, sem surpresas. |
| **🆓 Gratuito** | Sempre será. LTools é um projeto da comunidade para a comunidade. |
| **⚡ Rápido** | Interface nativa, sem Electron. Leve e responsivo. |
| **🧩 Modular** | Arquitetura baseada em plugins. Use só o que precisa. |
| **🎨 Moderno** | UI construída com Avalonia UI — fluida, bonita e consistente. |
| **🌍 Multiplataforma** | Windows, Linux e macOS com a mesma experiência. |
| **🤝 Comunitário** | Mantido por desenvolvedores para desenvolvedores. |

---

## 🖥️ Requisitos do Sistema

### Mínimos

| Componente | Especificação |
|------------|---------------|
| **Sistema Operacional** | Windows 10 21H2+, Ubuntu 22.04+, Fedora 38+, macOS 13+ |
| **Arquitetura** | x64 (ARM64 experimental) |
| **.NET Runtime** | .NET 9 Runtime |
| **RAM** | 256 MB |
| **Armazenamento** | 50 MB |
| **Resolução de tela** | 1024 × 768 |
| **Laravel** | 9.x, 10.x, 11.x (12.x experimental) |
| **PHP** | 8.1+ |
| **MySQL (opcional)** | 8.0+ (para o SQL Debugger) |

### Recomendados

| Componente | Especificação |
|------------|---------------|
| **RAM** | 1 GB+ |
| **Armazenamento** | 200 MB |
| **Resolução de tela** | 1366 × 768+ |
| **Composer** | 2.x |
| **Git** | 2.x |
| **Node.js** | 18.x+ (para detecção de versão) |
| **Docker** | 24.x+ (para detecção de versão) |

### Sistemas Operacionais Suportados

| SO | Status |
|----|--------|
| 🪟 Windows 10+ | ✅ Testado |
| 🐧 Ubuntu 22.04+ | ✅ Testado |
| 🐧 Fedora 38+ | ✅ Testado |
| 🍎 macOS 13+ (Ventura) | ✅ Compilado |
| 🍎 macOS 14+ (Sonoma) | ✅ Compilado |
| 🐧 Arch Linux | 🧪 Experimental |
| 🐧 Debian 12+ | 🧪 Experimental |

---

## 🚀 Funcionalidades

O LTools reúne **12 plugins** que cobrem as principais tarefas do dia a dia de um desenvolvedor Laravel:

| # | Plugin | Ícone | O que faz |
|---|--------|:-----:|-----------|
| 1 | **Dashboard** | 📊 | Visão geral do projeto com estatísticas |
| 2 | **Projetos** | 📁 | Localizador de projetos Laravel no disco |
| 3 | **Artisan** | ⚡ | Interface gráfica completa para comandos Artisan |
| 4 | **.env** | 🔐 | Editor seguro de arquivos de ambiente |
| 5 | **Composer** | 🧙 | Gerenciador de dependências Composer |
| 6 | **Cache** | 🗄️ | Limpeza de todos os tipos de cache Laravel |
| 7 | **Rotas** | 🗺️ | Visualizador e inspetor de rotas |
| 8 | **Migrations** | 🗄️ | Construtor visual de migrations |
| 9 | **Logs** | 📋 | Visualizador estruturado de logs |
| 10 | **Doctor** | 🩺 | Diagnóstico completo de saúde do projeto |
| 11 | **SQL Debug** | 🔍 | Monitor de queries MySQL em tempo real |
| 12 | **DB Diagram** | 📊 | Diagrama do banco reverso das migrations |

---

## 🧩 Plugins

### 📊 Dashboard

O **Dashboard** é a tela inicial do LTools. Quando você abre um projeto Laravel, ele exibe um resumo completo e imediato do estado do projeto.

```
📊 Dashboard
├── 🌐 Ferramentas Globais
│   ├── PHP     → 8.3.6
│   ├── Laravel → 11.0.5
│   ├── Composer → 2.7.1
│   ├── Git     → 2.45.0
│   ├── Node.js → 22.0.0
│   └── Docker  → 26.1.0
├── 📦 Projeto
│   ├── Nome    → meuarquivo
│   ├── Caminho → /home/user/projetos/meuarquivo
│   ├── Tamanho → 45.2 MB
│   └── Pacotes → 42 (require) + 18 (dev)
├── ⚙️ Ambiente
│   ├── Laravel → 11.0.5
│   ├── PHP     → 8.3.6
│   ├── APP_ENV → local
│   ├── APP_DEBUG → true
│   └── Drivers → cache (file), db (mysql), logs (stack)
└── 📁 Arquivos por Tipo
    ├── Models       → 24
    ├── Controllers  → 18
    ├── Migrations   → 32
    ├── Jobs         → 5
    ├── Services     → 12
    └── ... (18 categorias)
```

**Recursos:**
- Detecção automática de versões do PHP, Laravel, Composer, Git, Node.js e Docker
- Exibição de informações do ambiente via `php artisan about --json`
- Contagem de arquivos por tipo (models, controllers, migrations, jobs, etc.)
- Tamanho total do projeto (excluindo dependências)
- Atualização em tempo real ao trocar de projeto

---

### 📁 Projetos

Localize rapidamente todos os projetos Laravel no seu computador.

**Recursos:**
- Escaneamento recursivo de diretórios
- Detecção automática de projetos via `artisan`, `composer.json` e `.env`
- Exibição de versão Laravel, versão PHP, banco de dados, ambiente e tamanho
- Abrir diretório do projeto no explorador de arquivos com duplo clique

---

### ⚡ Artisan

Uma interface gráfica completa para executar comandos **php artisan**.

**Recursos:**
- 📜 Lista completa de comandos carregada dinamicamente via `artisan list --json`
- 🔍 Pesquisa em tempo real por nome ou descrição do comando
- 🌐 Descrições traduzidas para português brasileiro
- ⚙️ Exibição e preenchimento de argumentos e opções
- ⌨️ Modo de comando personalizado (digite qualquer comando Artisan)
- 🛡️ Proteção contra comandos perigosos (confirmação para `migrate:fresh`, `db:wipe`, etc.)
- 📋 Histórico de comandos executados com timestamps
- 📤 Saída em tempo real com remoção de códigos ANSI
- 📋 Cópia de saída para a área de transferência

---

### 🔐 .env (Env Manager)

Editor seguro para arquivos **.env**.

**Recursos:**
- Suporte a múltiplos arquivos: `.env`, `.env.example`, `.env.local`, `.env.dev`, `.env.qa`, `.env.prod`, `.env.staging`
- Edição inline com destaque de alterações não salvas
- 🔄 Backup automático antes de cada salvamento (com timestamp)
- 📦 Restauração do backup mais recente
- ➕ Criação de `.env` a partir de `.env.example` com um clique
- Troca rápida entre projetos

---

### 🧙 Composer

Gerenciador gráfico de dependências **Composer**.

**Recursos:**
- Lista completa de pacotes `require` e `require-dev` lidos do `composer.json`
- Botões de ação rápida: `install`, `update`, `require`, `remove`, `outdated`, `show --latest`
- Campo de entrada para nome do pacote (ex: `laravel/sanctum`)
- 📤 Saída em tempo real de todos os comandos
- 📋 Histórico de comandos executados
- 🔄 Atualização automática da lista de pacotes após operações

---

### 🗄️ Cache

Gerenciador de caches do Laravel.

**Recursos:**
- 🔄 Limpar todos os caches de uma só vez
- Botões individuais: Application, Config, Route, View e Event
- 📤 Saída em tempo real de cada comando
- Status de operação

---

### 🗺️ Rotas (Route Explorer)

Visualizador e inspetor completo de **rotas**.

**Recursos:**
- Carregamento de todas as rotas via `artisan route:list --json`
- 🎨 Código de cores por método HTTP (GET=azul, POST=verde, PUT=amarelo, DELETE=vermelho)
- 🔍 Filtro por método, texto, URI, nome, action ou middleware
- 🔁 Detecção de rotas duplicadas
- 📊 Estatísticas: total, nomeadas, anônimas, contagem por método
- 🏷️ Modos de agrupamento: prefixo URI, controller, middleware ou domínio
- 📄 Painel de detalhes: URI, nome, action, domain, middleware, vendor
- 📤 Exportação para CSV
- 📋 Cópia de rota para a área de transferência
- 🧹 Limpeza de filtros com um clique

---

### 🗄️ Migrations (Migration Studio)

Construtor visual de **migrations** — sem precisar escrever PHP.

**Recursos:**
- 🏗️ Construtor visual de colunas: adicione, remova e reordene campos
- 🎯 23 tipos de coluna: `string`, `integer`, `text`, `boolean`, `decimal`, `enum`, `foreignId`, `uuid`, `jsonb`, e mais
- ⚙️ Modificadores: nullable, unique, unsigned, autoIncrement, primary key
- 📏 Comprimento, precisão e escala para tipos relevantes
- 🔑 Chaves estrangeiras com `onDelete` e `onUpdate` (cascade, restrict, set null, no action)
- 📝 Código PHP gerado em tempo real
- 💾 Salvamento em `database/migrations/` com timestamp automático
- ▶️ Execução: `migrate`, `rollback`, `refresh`, `fresh`
- 📋 Status das migrations (executadas vs. pendentes)
- ⏪ Rollback seletivo até uma migration específica
- 📤 Log de execução em tempo real
- 🔍 **Comparador de Migrations**: detecta divergências entre arquivos de migração, banco de dados real e status do artisan
  - ✅ Migrations pendentes
  - ⚠️ Tabelas órfãs (existem no banco mas não têm migration)

---

### 📋 Logs (Log Viewer)

Visualizador e analisador de **logs do Laravel**.

**Recursos:**
- 📂 Lista de arquivos `.log` em `storage/logs/` com nome, tamanho e data
- 🔄 Watch automático com `FileSystemWatcher` (auto-refresh ao detectar alterações)
- 🔍 Parse estruturado: timestamp, ambiente, nível, mensagem e stack trace
- 👁️ Modo duplo: visualização **Raw** (texto completo) ou **Estruturada** (entradas parseadas)
- 🎯 Filtro por nível: ERROR, WARNING, INFO, DEBUG, CRITICAL, ALERT, EMERGENCY, NOTICE
- 🔎 Busca textual em mensagens e stack traces
- 📅 Filtro por intervalo de datas
- 🏷️ Badges coloridos por nível de severidade
- 📋 Cópia de entrada individual, entradas filtradas ou conteúdo bruto
- 📊 Contagem total e contagem filtrada

---

### 🩺 Doctor (Project Doctor)

Diagnóstico completo de **saúde do projeto Laravel** com 31 verificações e pontuação percentual.

**Categorias de Verificação:**

```
🟦 Essenciais (7)  → .env, composer.json, package.json, .gitignore, Dockerfile...
🔴 Segurança (4)   → APP_KEY, APP_DEBUG, APP_ENV, .env no .gitignore
🔵 Banco (3)       → DB_CONNECTION, DB_HOST, DB_DATABASE
🟡 Cache (4)       → CACHE_DRIVER, SESSION_DRIVER, config cache, route cache
🔵 E-mail (2)      → MAIL_MAILER, MAIL_FROM_ADDRESS
🟡 Queue (1)       → QUEUE_CONNECTION
🔵 Estrutura (3)   → Storage link, migrations pendentes, arquivos de rota
🟠 Ferramentas (6) → PHP, Composer, Git, Node.js, NPM, pacotes desatualizados
🔵 Docker (2)      → Laravel Sail, vendor/
```

**Recursos:**
- 📊 **Score geral** de 0 a 100% com classificação: Excelente ✅, Bom 👍, Regular ⚠️, Crítico ❌
- 🎯 Três níveis de severidade: 🔴 Crítico, 🟡 Warning, 🔵 Info
- 🔧 **Correção com um clique**: `key:generate`, `storage:link`, `composer install` e mais
- 🔄 **Corrigir tudo**: executa todas as correções seguras em sequência
- 🔁 Reavaliação automática após correções
- 📋 Sugestões detalhadas para cada verificação

---

### 🔍 SQL Debug (SQL Debugger)

Monitor de **queries MySQL em tempo real**.

**Recursos:**
- 🔌 Conexão automática lendo credenciais do `.env`
- 📡 Monitoramento via `mysql.general_log` com polling a cada 400ms
- 🔴 Queries lentas (>1000ms) destacadas em vermelho
- 🔎 Busca textual nas queries capturadas
- 🎯 Filtro "Somente lentas"
- 📤 Exportação de queries para arquivo `.txt`
- ⏯️ Iniciar/Parar monitoramento
- 🔄 Reconexão com atualização das credenciais
- 🧹 Limpeza da lista de queries
- ⚠️ Filtro inteligente que exclui queries internas (SET, SHOW, FLUSH) e de outros bancos

---

### 📊 DB Diagram (Database Diagram)

**Diagrama de banco de dados** gerado a partir das migrations.

**Recursos:**
- 🔍 Análise estática de todos os arquivos PHP em `database/migrations/`
- 🏗️ Extração de tabelas via `Schema::create()` e `Schema::table()`
- 🧩 Colunas com tipo, primary key e nullable
- 🔗 Detecção de chaves estrangeiras por:
  1. `->constrained('tabela')`
  2. `->references('col')->on('tabela')`
  3. Inferência automática: `user_id` → `users.id`
- 🔄 Relacionamentos entre tabelas
- 👆 Clique em qualquer tabela para ver detalhes
- 📊 Contagem de tabelas, colunas e relacionamentos
- 🔄 Reanálise automática ao trocar de projeto

---

## 🏗️ Arquitetura

```
┌──────────────────────────────────────────────────────────┐
│                     LTools.UI                            │
│              (Aplicação Avalonia Desktop)                │
│                                                          │
│  ┌─────────────┐  ┌──────────────┐  ┌────────────────┐  │
│  │  MainWindow  │  │  ViewModels  │  │   Converters   │  │
│  │  + Sidebar   │  │  (MVVM)      │  │  (IValueConv.) │  │
│  └──────┬───────┘  └──────┬───────┘  └────────────────┘  │
│         │                 │                               │
│  ┌──────┴─────────────────┴──────────────────────────┐   │
│  │              PluginLoader (Reflection)             │   │
│  │         Carrega DLLs da pasta plugins/             │   │
│  └────────────────────┬───────────────────────────────┘   │
└───────────────────────┼───────────────────────────────────┘
                        │
┌───────────────────────┼───────────────────────────────────┐
│              LTools.Core                                  │
│                                                           │
│  ┌────────────┐  ┌──────────────┐  ┌──────────────────┐  │
│  │ Interfaces │  │   Models     │  │    Services       │  │
│  │            │  │              │  │                   │  │
│  │ IPluginLoader│  │ PluginResult│  │ PluginLoader     │  │
│  │ IProcessRun.│  │ ProjectInfo │  │ ProcessRunner    │  │
│  │ ILaravelDet.│  │ PluginCont. │  │ LaravelDetector  │  │
│  │ IConfigMgr  │  │             │  │ ConfigManager    │  │
│  │ IMessenger  │  │             │  │ ThemeManager     │  │
│  │ IThemeMgr   │  │             │  │ Messenger        │  │
│  └────────────┘  └──────────────┘  └──────────────────┘  │
└───────────────────────────────────────────────────────────┘
                        │
              ┌─────────┴─────────┐
              │                   │
     ┌────────┴────────┐  ┌──────┴──────┐
     │  ProjectContext  │  │  Plugins/   │
     │   (Singleton)    │  │   (DLLs)    │
     └─────────────────┘  └─────────────┘
```

### Tecnologias

| Tecnologia | Versão | Finalidade |
|------------|--------|------------|
| C# | 13 | Linguagem principal |
| .NET | 9.0 | Runtime e BCL |
| Avalonia UI | 12.x | Framework de interface gráfica multiplataforma |
| CommunityToolkit.Mvvm | 8.4 | MVVM source generators e componentes |
| MySqlConnector | 2.x | Conexão MySQL (SQL Debugger) |

---

## 📦 Instalação

### Pré-requisitos

- [.NET 9 Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) instalado
- Um projeto Laravel (9.x, 10.x ou 11.x)
- PHP 8.1+ no PATH

### Download

1. Acesse a [página de releases](https://github.com/viniciuswerneck/ltools/releases)
2. Baixe o arquivo correspondente ao seu sistema operacional:
   - 🪟 Windows: `LTools-win-x64.zip`
   - 🐧 Linux: `LTools-linux-x64.tar.gz`
   - 🍎 macOS: `LTools-osx-x64.tar.gz`
3. Extraia em uma pasta de sua preferência
4. Execute o binário:

```bash
# Windows
LTools.UI.exe

# Linux / macOS
./LTools.UI
```

### Compilando do código fonte

```bash
# Clone o repositório
git clone https://github.com/viniciuswerneck/ltools.git
cd LTools

# Compile toda a solução
dotnet build LTools.sln

# Execute
dotnet run --project src/LTools.UI/LTools.UI.csproj

# Publique como executável único
dotnet publish src/LTools.UI/LTools.UI.csproj -c Release -o dist
```

---

## 🤝 Como Contribuir

O LTools é um projeto **100% open source** e toda contribuição é bem-vinda!

### Formas de contribuir

- 🐛 **Reportar bugs** — Abra uma [issue](https://github.com/viniciuswerneck/ltools/issues)
- 💡 **Sugerir funcionalidades** — Compartilhe suas ideias
- 🔧 **Enviar PRs** — Corrija bugs, adicione plugins, melhore a documentação
- 📖 **Melhorar a documentação** — README, wiki, tutoriais
- 🌐 **Traduções** — Ajude a traduzir a interface para outros idiomas
- ⭐ **Dar feedback** — Use o projeto e compartilhe sua experiência

### Como criar um fork

```bash
# Faça um fork no GitHub e então clone seu fork
git clone https://github.com/seu-usuario/LTools.git
cd LTools

# Crie uma branch para sua funcionalidade
git checkout -b minha-feature

# Faça suas alterações e commit
git commit -m "feat: adiciona nova funcionalidade"

# Envie para seu fork
git push origin minha-feature

# Abra um Pull Request no repositório original
```

### Criando seu próprio plugin

O LTools usa um sistema de plugins baseado em DLLs. Qualquer um pode criar um plugin:

```csharp
using Avalonia.Controls;
using LTools.Core.Interfaces;

namespace MeuPlugin;

public class MeuPlugin : ILToolsPlugin
{
    public string Name => "Meu Plugin";
    public string Icon => "🧩";

    public Task<PluginResult> ExecuteAsync(PluginContext context)
    {
        return Task.FromResult(new PluginResult { Success = true });
    }

    public UserControl GetView()
    {
        return new MeuPluginView { DataContext = new MeuPluginViewModel() };
    }
}
```

Basta compilar como uma Class Library (.NET 9) e colocar a DLL na pasta `plugins/` ao lado do executável.

---

## 🗂️ Estrutura do Projeto

```
LTools/
├── src/
│   ├── LTools.Core/           # Class Library — interfaces e serviços
│   │   ├── Interfaces/        #   Contratos do sistema
│   │   ├── Models/            #   Modelos compartilhados
│   │   ├── Services/          #   Implementações (PluginLoader, ProcessRunner, etc.)
│   │   └── Extensions/        #   Métodos de extensão
│   └── LTools.UI/             # Aplicação Avalonia Desktop
│       ├── ViewModels/        #   ViewModels (MainWindowViewModel, etc.)
│       ├── Views/             #   Janelas e controles XAML
│       └── Converters/        #   Value converters
├── plugins/                   # Plugins (cada um em sua própria DLL)
│   ├── Dashboard/
│   ├── ProjectManager/
│   ├── ArtisanGui/
│   ├── EnvManager/
│   ├── ComposerManager/
│   ├── CacheExplorer/
│   ├── RouteExplorer/
│   ├── MigrationStudio/
│   ├── LogViewer/
│   ├── ProjectDoctor/
│   ├── SqlDebugger/
│   └── DatabaseDiagram/
├── LTools.sln
├── readme.md
└── LICENSE
```

Cada plugin segue a mesma estrutura interna:

```
plugins/NomeDoPlugin/
├── NomeDoPluginPlugin.cs       # Implementação de ILToolsPlugin
├── NomeDoPlugin.csproj         # Class Library → plugins/
├── ViewModels/
│   └── NomeDoPluginViewModel.cs
├── Views/
│   ├── NomeDoPluginView.axaml
│   └── NomeDoPluginView.axaml.cs
└── Models/                     # (opcional)
    └── ...
```

---

## 📄 Licença

Este projeto está licenciado sob a **MIT License** — veja o arquivo [LICENSE](LICENSE) para mais detalhes.

---

## 💬 Comunidade

- 🐛 **Reporte bugs**: [github.com/viniciuswerneck/ltools/issues](https://github.com/viniciuswerneck/ltools/issues)
- ⭐ **Deixe uma estrela**: [github.com/viniciuswerneck/ltools](https://github.com/viniciuswerneck/ltools)
- 🔄 **Faça um fork**: contribua com código, plugins, documentação ou traduções

---

<div align="center">
  <br/>
  <p>
    Desenvolvido por <a href="https://lab.werneck.dev.br/"><strong>Werneck Lab</strong></a> — inovação open source para o ecossistema Laravel<br/>
    Com ❤️ pela comunidade Laravel<br/>
    <strong>LTools</strong> — <em>The Ultimate Desktop Toolkit for Laravel Developers</em>
  </p>
  <br/>
  <p>
    <a href="https://github.com/viniciuswerneck/ltools">
      <img src="https://img.shields.io/badge/⭐ Star-0052CC?style=for-the-badge" alt="Star us on GitHub"/>
    </a>
    <a href="https://github.com/viniciuswerneck/ltools/fork">
      <img src="https://img.shields.io/badge/🍴 Fork-24292f?style=for-the-badge" alt="Fork this repository"/>
    </a>
  </p>
  <br/>
</div>
