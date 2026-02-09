# Sistema de Controle de Estacionamento

Sistema web para gerenciamento de estacionamento com registro de entrada/saída de veículos e cálculo automático de valores baseado em tabelas de preço configuráveis.

## Stack Utilizada

- **Linguagem:** C# 13
- **Framework:** .NET 10 / ASP.NET Core MVC
- **ORM:** Entity Framework Core 10
- **Banco de Dados:** SQLite
- **Testes:** xUnit, Moq, FluentAssertions
- **Frontend:** Tailwind CSS 4
- **Extras:** AutoMapper, FluentValidation, Swagger

## Arquitetura

O projeto segue os princípios de Clean Architecture com separação em camadas:

```
src/
├── ControleEstacionamento.Domain/        # Entidades e Interfaces
├── ControleEstacionamento.Application/   # Serviços, DTOs, Validações
├── ControleEstacionamento.Infrastructure/ # EF Core, Repositórios
└── ControleEstacionamento.Web/           # Controllers, Views

tests/
└── ControleEstacionamento.Tests/         # Testes Unitários
```

### Design Patterns Utilizados

- **Repository Pattern** - Abstração do acesso a dados
- **Unit of Work** - Gerenciamento de transações
- **Strategy Pattern** - Cálculo de preços
- **Service Layer** - Lógica de negócio isolada
- **Dependency Injection** - Inversão de controle

## Regras de Negócio

### Cálculo de Preço
- **Até 30 minutos:** Metade do valor da hora inicial
- **De 31 a 60 minutos:** Valor da hora inicial completo
- **Acima de 1 hora:** Hora inicial + (horas adicionais × valor hora adicional)
- **Tolerância:** 10 minutos por hora adicional

### Exemplos de Cobrança
| Tempo | Cobrança |
|-------|----------|
| 30 min | Meia hora inicial |
| 1h | 1 hora inicial |
| 1h10 | 1 hora (tolerância) |
| 1h15 | 2 horas |
| 2h05 | 2 horas (tolerância) |
| 2h15 | 3 horas |

## Instalação e Execução

### Pré-requisitos
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (para Tailwind CSS)

### Passos

```bash
# Clonar repositório
git clone <url-do-repositorio>
cd controleestacionamento

# Restaurar dependências .NET
dotnet restore

# Instalar dependências npm (Tailwind CSS)
cd src/ControleEstacionamento.Web
npm install
npm run build:css
cd ../..

# Executar testes
dotnet test

# Executar aplicação
dotnet run --project src/ControleEstacionamento.Web
```

A aplicação estará disponível em: `https://localhost:5001` ou `http://localhost:5000`

### Swagger

A documentação da API está disponível em ambiente de desenvolvimento:
`https://localhost:5001/swagger`

## Testes

O projeto utiliza TDD (Test-Driven Development) com cobertura das regras de cálculo de preço.

```bash
# Executar todos os testes
dotnet test

# Executar com verbosidade
dotnet test --verbosity normal

# Executar com cobertura de código
dotnet test --collect:"XPlat Code Coverage"
```

### Testes Implementados
- 13 testes unitários para cálculo de preço
- Cenários de tolerância
- Valores personalizados
- Casos limite

## Funcionalidades

- ✅ Registro de entrada de veículos
- ✅ Registro de saída com cálculo automático
- ✅ Gerenciamento de tabelas de preço
- ✅ Validação de vigência de tabelas
- ✅ Histórico de movimentações
- ✅ Painel de controle com estatísticas
- ✅ Interface responsiva com Tailwind CSS

## Estrutura do Banco de Dados

### VeiculosEstacionados
| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | int | Chave primária |
| Placa | string(10) | Placa do veículo |
| DataHoraEntrada | DateTime | Momento da entrada |
| DataHoraSaida | DateTime? | Momento da saída |
| ValorCobrado | decimal? | Valor calculado |

### TabelasPreco
| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | int | Chave primária |
| DataInicioVigencia | DateTime | Início da vigência |
| DataFimVigencia | DateTime | Fim da vigência |
| ValorHoraInicial | decimal | Valor primeira hora |
| ValorHoraAdicional | decimal | Valor horas extras |

## Como Usar

1. **Cadastrar Tabela de Preço:** Acesse "Preços" e crie uma tabela com vigência atual
2. **Registrar Entrada:** Clique em "Registrar Entrada" e informe a placa
3. **Registrar Saída:** Na lista de veículos, clique em "Registrar Saída"
4. **Consultar Histórico:** Acesse "Histórico" para ver todas as movimentações

---

>  This is a challenge by [Coodesh](https://coodesh.com/)
