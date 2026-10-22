
import { useEffect, useState, type FormEvent } from 'react'
import { demandService } from './services/demandService'
import Dashboard from './components/Dashboard'
import type {
  CreateDemandRequest,
  Demand,
  DemandStatus,
} from './types/demand'
import './App.css'

function getToday() {
  const date = new Date()
  const localDate = new Date(
    date.getTime() - date.getTimezoneOffset() * 60_000
  )

  return localDate.toISOString().slice(0, 10)
}

function createInitialForm(): CreateDemandRequest {
  return {
    title: '',
    description: '',
    requestDate: getToday(),
    sector: '',
    status: 'Pending',
  }
}

const statusLabels: Record<DemandStatus, string> = {
  Pending: 'Pendente',
  InProgress: 'Em andamento',
  Completed: 'Concluída',
}

function App() {
  const [demands, setDemands] = useState<Demand[]>([])
  const [form, setForm] = useState<CreateDemandRequest>(
    createInitialForm
  )

  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [updatingIds, setUpdatingIds] = useState<string[]>([])

  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  useEffect(() => {
    let active = true

    async function fetchDemands() {
      try {
        const data = await demandService.getAll()

        if (active) {
          setDemands(data)
        }
      } catch {
        if (active) {
          setError('Não foi possível carregar as demandas.')
        }
      } finally {
        if (active) {
          setLoading(false)
        }
      }
    }

    void fetchDemands()

    return () => {
      active = false
    }
  }, [])

  async function loadDemands() {
    try {
      setLoading(true)

      const data = await demandService.getAll()
      setDemands(data)

      return true
    } catch {
      setError('Não foi possível atualizar a lista de demandas.')
      return false
    } finally {
      setLoading(false)
    }
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    setError('')
    setSuccess('')

    if (
      !form.title.trim() ||
      !form.description.trim() ||
      !form.requestDate ||
      !form.sector.trim()
    ) {
      setError('Preencha todos os campos obrigatórios.')
      return
    }

    try {
      setSaving(true)

      const created = await demandService.create(form)

      // Atualiza a tabela e o dashboard sem outra requisição.
      setDemands(current => [...current, created])

      setForm(createInitialForm())
      setSuccess('Demanda cadastrada com sucesso!')
    } catch {
      setError('Não foi possível cadastrar a demanda.')
    } finally {
      setSaving(false)
    }
  }

  async function handleStatusChange(
    id: string,
    newStatus: DemandStatus
  ) {
    if (updatingIds.includes(id)) {
      return
    }

    const currentDemand = demands.find(demand => demand.id === id)

    if (!currentDemand || currentDemand.status === newStatus) {
      return
    }

    setError('')
    setSuccess('')

    setUpdatingIds(current => [...current, id])

    try {
      const updated = await demandService.updateStatus(id, newStatus)

      // Atualiza a demanda com a resposta real da API.
      // O Dashboard recebe os novos dados automaticamente.
      setDemands(current =>
        current.map(demand =>
          demand.id === id ? updated : demand
        )
      )

      setSuccess('Status atualizado com sucesso!')
    } catch {
      // Como não alteramos o estado antes da resposta,
      // o status anterior permanece selecionado.
      setError('Não foi possível atualizar o status da demanda.')
    } finally {
      setUpdatingIds(current =>
        current.filter(updatingId => updatingId !== id)
      )
    }
  }

  return (
    <main className="container">
      <header>
        <h1>Internal Requests</h1>
        <p>Gestão de demandas internas</p>
      </header>

      <Dashboard demands={demands} />

      <section className="panel">
        <h2>Nova demanda</h2>

        <form onSubmit={handleSubmit}>
          <label>
            Título
            <input
              required
              maxLength={200}
              value={form.title}
              onChange={event =>
                setForm({
                  ...form,
                  title: event.target.value,
                })
              }
            />
          </label>

          <label>
            Descrição
            <textarea
              required
              maxLength={2000}
              value={form.description}
              onChange={event =>
                setForm({
                  ...form,
                  description: event.target.value,
                })
              }
            />
          </label>

          <label>
            Data de solicitação
            <input
              type="date"
              required
              value={form.requestDate}
              onChange={event =>
                setForm({
                  ...form,
                  requestDate: event.target.value,
                })
              }
            />
          </label>

          <label>
            Setor
            <input
              required
              maxLength={100}
              value={form.sector}
              onChange={event =>
                setForm({
                  ...form,
                  sector: event.target.value,
                })
              }
            />
          </label>

          <label>
            Status
            <select
              value={form.status}
              onChange={event =>
                setForm({
                  ...form,
                  status: event.target.value as DemandStatus,
                })
              }
            >
              <option value="Pending">Pendente</option>
              <option value="InProgress">Em andamento</option>
              <option value="Completed">Concluída</option>
            </select>
          </label>

          <p className="hint">
            A prioridade será definida automaticamente pela descrição.
          </p>

          <button type="submit" disabled={saving}>
            {saving ? 'Salvando...' : 'Cadastrar demanda'}
          </button>
        </form>
      </section>

      <section className="panel">
        <div className="demands-heading">
          <h2>Demandas cadastradas</h2>

          <button
            type="button"
            className="refresh-button"
            disabled={loading}
            onClick={() => {
              setError('')
              setSuccess('')
              void loadDemands()
            }}
          >
            {loading ? 'Atualizando...' : 'Atualizar lista'}
          </button>
        </div>

        {error && (
          <p role="alert" className="error">
            {error}
          </p>
        )}

        {success && (
          <p role="status" className="success">
            {success}
          </p>
        )}

        {loading ? (
          <p>Carregando...</p>
        ) : demands.length === 0 ? (
          <p>Nenhuma demanda cadastrada.</p>
        ) : (
          <div className="table-wrapper">
            <table>
              <thead>
                <tr>
                  <th>Título</th>
                  <th>Setor</th>
                  <th>Status</th>
                  <th>Prioridade</th>
                  <th>Data</th>
                </tr>
              </thead>

              <tbody>
                {demands.map(demand => {
                  const updating = updatingIds.includes(demand.id)

                  return (
                    <tr key={demand.id}>
                      <td>{demand.title}</td>
                      <td>{demand.sector}</td>

                      <td>
                        <div className="status-control">
                          <select
                            className="status-select"
                            aria-label={`Alterar status de ${demand.title}`}
                            value={demand.status}
                            disabled={updating}
                            onChange={event => {
                              void handleStatusChange(
                                demand.id,
                                event.target.value as DemandStatus
                              )
                            }}
                          >
                            {Object.entries(statusLabels).map(
                              ([value, label]) => (
                                <option key={value} value={value}>
                                  {label}
                                </option>
                              )
                            )}
                          </select>

                          {updating && (
                            <span className="status-saving">
                              Salvando...
                            </span>
                          )}
                        </div>
                      </td>

                      <td>
                        <span
                          className={
                            demand.priority === 'High'
                              ? 'high'
                              : 'normal'
                          }
                        >
                          {demand.priority === 'High'
                            ? 'Alta'
                            : 'Normal'}
                        </span>
                      </td>

                      <td>
                        {demand.requestDate.slice(0, 10)}
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </main>
  )
}

export default App
