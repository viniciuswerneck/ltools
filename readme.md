# LTools

> The Ultimate Desktop Toolkit for Laravel Developers

## Visão Geral

O **LTools** é uma suíte de ferramentas desktop desenvolvida para aumentar a produtividade de desenvolvedores Laravel.

Ao invés de abrir diversos programas (terminal, editor, navegador, banco de dados, monitor de logs...), o LTools centraliza tudo em uma única aplicação moderna.

O projeto será desenvolvido utilizando **C#**, **.NET** e **Avalonia UI**, permitindo execução em Windows, Linux e macOS.

---

# Objetivos

* Centralizar ferramentas utilizadas diariamente por desenvolvedores Laravel.
* Reduzir o uso do terminal para tarefas comuns.
* Automatizar diagnósticos de projetos.
* Fornecer uma interface rápida, bonita e intuitiva.
* Ser totalmente Open Source.

---

# Tecnologias

## Linguagem

* C#

## Framework

* .NET 9

## Interface

* Avalonia UI

## Arquitetura

* MVVM
* Dependency Injection
* Plugin Based
* SOLID

---

# Estrutura do Projeto

```text
LTools

├── Core
├── UI
├── Plugins
│
├── Artisan
├── RouteExplorer
├── LogViewer
├── SqlDebugger
├── QueueMonitor
├── Scheduler
├── DatabaseDiagram
├── EnvManager
├── CacheExplorer
├── ProjectDoctor
├── ComposerManager
├── DockerManager
└── VirtualHosts
```

---

# Núcleo (Core)

O Core é responsável por:

* detectar projetos Laravel
* carregar plugins
* gerenciar configurações
* controlar temas
* executar processos
* comunicação entre módulos

Todos os módulos utilizarão os serviços do Core.

---

# Interface

A aplicação possuirá uma única janela principal.

```
┌────────────────────────────────────────────┐
│ LTools                                     │
├──────────────┬─────────────────────────────┤
│ Dashboard    │                             │
│ Projetos     │                             │
│ Artisan      │                             │
│ Rotas        │                             │
│ Banco        │                             │
│ Logs         │                             │
│ Queue        │                             │
│ Scheduler    │                             │
│ SQL          │                             │
│ Cache        │                             │
│ .env         │                             │
│ Doctor       │                             │
│ Config       │                             │
└──────────────┴─────────────────────────────┘
```

---

# Roadmap

---

# Módulo 01

## Dashboard

Funções

* Projetos recentes
* PHP instalado
* Laravel instalado
* Composer
* Git
* Docker
* Atualizações disponíveis

---

# Módulo 02

## Gerenciador de Projetos

Detecta automaticamente

```
artisan

composer.json

.env

routes

storage

database
```

Exibe informações como

* versão Laravel
* versão PHP
* banco utilizado
* ambiente
* tamanho do projeto

---

# Módulo 03

## Artisan GUI

Executa automaticamente

```
php artisan list --format=json
```

Funcionalidades

* pesquisar comandos
* executar comandos
* salvar favoritos
* histórico
* parâmetros
* saída em tempo real

---

# Módulo 04

## Route Explorer

Executa

```
php artisan route:list --json
```

Permite

* pesquisar
* filtrar middleware
* filtrar controller
* exportar documentação
* visualizar parâmetros

---

# Módulo 05

## Log Viewer

Monitora

```
storage/logs
```

Recursos

* atualização automática
* filtros
* exceções
* copiar stacktrace
* pesquisar

---

# Módulo 06

## Database Diagram

Conecta ao banco e desenha automaticamente

* tabelas
* relacionamentos
* chaves estrangeiras
* índices

Possibilidade de exportar

* PNG
* SVG
* PDF

---

# Módulo 07

## SQL Debugger

Plugin Laravel responsável por transmitir queries em tempo real.

A aplicação exibirá

* SQL
* tempo
* bindings
* explain
* queries duplicadas
* consultas lentas

---

# Módulo 08

## Queue Monitor

Monitoramento completo das filas.

Recursos

* Jobs em execução
* Jobs falhados
* Tempo médio
* Reiniciar workers
* Limpar filas

---

# Módulo 09

## Scheduler

Lista todos os

```
Schedule::command()

Schedule::job()

Schedule::call()
```

Mostra

* próxima execução
* última execução
* frequência

---

# Módulo 10

## Env Manager

Gerenciamento de ambientes.

Funções

* editar .env
* comparar ambientes
* backup
* troca rápida

```
.env

.env.local

.env.dev

.env.qa

.env.prod
```

---

# Módulo 11

## Cache Explorer

Visualização dos caches.

Compatível

* File
* Redis
* Memcached

Funções

* pesquisar
* remover chave
* limpar cache

---

# Módulo 12

## Project Doctor

O módulo mais importante.

Analisa automaticamente o projeto.

Verificações

* APP_KEY
* APP_DEBUG
* Storage Link
* Config Cache
* Route Cache
* Composer
* PHP
* Extensões
* Permissões
* Queue
* Scheduler
* Banco
* Cache
* Logs
* SSL
* Variáveis não utilizadas
* Migrations pendentes
* Packages desatualizados

Ao final gera

```
Projeto saudável

91%

✔ 48 verificações

⚠ 7 avisos

❌ 2 problemas críticos
```

---

# Módulo 13

## Composer Manager

Interface gráfica para

* install
* update
* require
* remove
* outdated

---

# Módulo 14

## Docker Manager

Detecta Docker Compose.

Permite

* iniciar
* parar
* rebuild
* visualizar logs

---

# Módulo 15

## Virtual Hosts

Integração completa com o Virtual Hosts Manager.

Permite

* criar host
* editar
* remover
* SSL
* abrir navegador

---

# Sistema de Plugins

Cada módulo poderá ser carregado dinamicamente.

Estrutura

```
Plugins/

Artisan.dll

RouteExplorer.dll

Doctor.dll

Logs.dll
```

Facilitando futuras expansões.

---

# Objetivo Final

Criar a melhor ferramenta desktop gratuita para desenvolvedores Laravel.

Não pretende substituir o VS Code.

Não pretende substituir o PHPStorm.

Ela será um centro de operações para projetos Laravel.

---

# Filosofia

* Open Source
* Gratuito
* Rápido
* Modular
* Moderno
* Fácil de usar
* Mantido pela comunidade

---

# Nome Oficial

**LTools**

*"The Ultimate Desktop Toolkit for Laravel Developers"*
