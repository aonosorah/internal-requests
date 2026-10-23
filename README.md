# Internal Requests

**Sistema web para registro, consulta e acompanhamento de demandas internas.**

Projeto full stack desenvolvido como solução para um desafio técnico de **Analista de Desenvolvimento Júnior**, com foco em regras de negócio, validação de dados, organização do código e facilidade de execução.

## Visão geral

A aplicação centraliza solicitações de diferentes setores e atribui automaticamente a prioridade de cada demanda com base em sua descrição. Além do cadastro e da consulta, inclui acompanhamento de status e um painel de indicadores para facilitar a visualização das solicitações.

### Funcionalidades implementadas

- Cadastro de demandas com **título, descrição, data de solicitação, setor e status**.
- **Prioridade automática**, sem necessidade de preenchimento manual.
- Validação de campos obrigatórios e rejeição de dados inválidos.
- Listagem e consulta individual das demandas.
- Atualização do status de uma demanda.
- Painel visual com indicadores das demandas.
- Persistência dos registros em banco de dados SQLite.
- Testes unitários e de integração da API.

> **Em evolução:** filtros e paginação. A estrutura inicial dessas melhorias está em desenvolvimento; elas não são requisitos para a utilização das funcionalidades principais.

## Tecnologias

| Camada | Tecnologias |
| --- | --- |
| Backend | C#, .NET 8, ASP.NET Core Web API |
| Persistência | Entity Framework Core, SQLite |
| Frontend | React, TypeScript, Vite |
| Visualização | Recharts |
| Testes | xUnit, Python (testes de integração HTTP) |
| Versionamento | Git e GitHub |

## Arquitetura e organização

O backend está organizado em camadas para separar regras de negócio, casos de uso, persistência e exposição da API:

```text
internal-requests/
├── backend/
│   ├── InternalRequests.Domain/          # Entidades e enumerações
│   ├── InternalRequests.Application/     # Serviços, DTOs e interfaces
│   ├── InternalRequests.Infrastructure/  # EF Core e repositórios
│   └── InternalRequests.Api/             # Controllers e configuração da API
├── frontend/
│   └── src/                              # Interface React/TypeScript
├── tests/
│   └── InternalRequests.Tests/          # Testes unitários com xUnit
└── scripts/
    └── smoke-test.py                    # Testes de integração HTTP
```

**Responsabilidades principais:**

- **Domain:** representa as demandas e os valores de domínio.
- **Application:** concentra a lógica de aplicação, os contratos e a transformação de dados.
- **Infrastructure:** implementa o acesso aos dados com Entity Framework Core.
- **Api:** disponibiliza os endpoints REST e trata as requisições HTTP.
- **Frontend:** apresenta formulários, consultas, acompanhamento de status e indicadores.

Essa separação permite evoluir a interface ou a persistência sem concentrar todas as responsabilidades em um único componente.

## Regras de negócio

### Validação do cadastro

O cadastro exige os campos de entrada obrigatórios. Valores ausentes ou inválidos são rejeitados pela API, evitando o armazenamento de demandas incompletas.

### Classificação automática da prioridade

A prioridade é determinada **exclusivamente pela descrição**:

| Conteúdo da descrição | Prioridade |
| --- | --- |
| Contém `urgente` | **Alta** |
| Contém `erro` | **Alta** |
| Não contém nenhum desses termos | **Normal** |

A verificação não diferencia letras maiúsculas e minúsculas. A prioridade é calculada no backend, em vez de depender de uma escolha manual na interface.

**Exemplos:**

- `Erro ao emitir relatório` → **Alta**
- `Solicitação urgente de acesso` → **Alta**
- `Atualizar cadastro de fornecedor` → **Normal**

## Como executar localmente

### Pré-requisitos

- .NET SDK **8**
- Node.js e npm compatíveis com o projeto
- Git (para clonar o repositório)

Abra dois terminais na pasta raiz `internal-requests`.

**Terminal 1 — Backend**

```bash
dotnet restore backend/InternalRequests.Api
dotnet run --project backend/InternalRequests.Api --urls http://localhost:5000
```

API disponível em `http://localhost:5000`.

**Terminal 2 — Frontend**

```bash
cd frontend
npm install
npm run dev
```

Abra o endereço informado pelo Vite no terminal (normalmente `http://localhost:5173`). O frontend utiliza o proxy de desenvolvimento para acessar os endpoints `/api`.

> **Banco de dados:** a aplicação utiliza SQLite. Caso o banco ainda não esteja inicializado, verifique a configuração do `AppDbContext` e as migrações do projeto antes da primeira execução.

## Endpoints da API

| Método | Rota | Finalidade |
| --- | --- | --- |
| `POST` | `/api/demands` | Cadastrar uma demanda |
| `GET` | `/api/demands` | Listar demandas |
| `GET` | `/api/demands/{id}` | Consultar uma demanda pelo ID |
| `PATCH` | `/api/demands/{id}/status` | Atualizar o status |

A API utiliza códigos HTTP apropriados, incluindo `201` para criação, `200` para consultas e atualizações, `400` para entradas inválidas e `404` para demandas não encontradas.

## Testes e qualidade

### Testes unitários

```bash
dotnet test tests/InternalRequests.Tests --verbosity normal
```

**Última execução validada:** 18 testes aprovados, nenhum erro.

### Testes de integração

Com a API em execução na porta `5000`:

```bash
python3 scripts/smoke-test.py
```

**Última execução validada:** 16 verificações aprovadas, incluindo cadastro, consulta, validações, classificação de prioridade, 20 cadastros simultâneos, atualização de status e preservação dos dados.

### Build e análise estática

```bash
dotnet build backend/InternalRequests.Api
cd frontend
npm run lint
npm run build
```

**Última execução validada:** backend e frontend compilados com sucesso; lint sem erros. O build do Vite apresentou apenas um aviso não bloqueante sobre tamanho do bundle.

## Decisões técnicas

- **SQLite:** reduz a complexidade de instalação e permite executar o projeto sem provisionar um servidor de banco de dados externo.
- **Entity Framework Core:** abstrai operações de persistência e facilita a manutenção do acesso a dados.
- **Separação em camadas:** mantém as regras de negócio desacopladas da interface e dos detalhes de infraestrutura.
- **Prioridade calculada no backend:** garante que a regra seja aplicada de maneira consistente, independentemente do cliente HTTP utilizado.
- **React com TypeScript:** favorece a organização da interface e a segurança de tipos.
- **Testes automatizados:** verificam tanto regras isoladas quanto o comportamento real dos endpoints.
- **Dashboard:** acrescenta uma visão operacional das demandas, como diferencial em relação ao cadastro e à consulta solicitados.

## Próximas melhorias

- Concluir filtros por atributos das demandas.
- Implementar paginação para consultas com maior volume de registros.
- Refinar a experiência de uso e otimizar o bundle do frontend.

---

**Objetivo da entrega:** demonstrar uma solução funcional, testável e de fácil manutenção, com atenção às regras de negócio e à clareza das decisões técnicas.
