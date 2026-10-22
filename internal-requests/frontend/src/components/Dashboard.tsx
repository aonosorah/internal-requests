import { useMemo } from 'react'
import {
  BarChart, Bar, XAxis, YAxis, Tooltip,
  ResponsiveContainer, CartesianGrid,
  PieChart, Pie, Cell, Legend
} from 'recharts'
import type { Demand } from '../types/demand'
import './Dashboard.css'

interface DashboardProps {
  demands: Demand[]
}

const COLORS = ['#6366f1', '#10b981', '#f59e0b', '#ef4444']

const STATUS_LABELS = {
  Pending: 'Pendente',
  InProgress: 'Em andamento',
  Completed: 'Concluída',
}

export default function Dashboard({ demands }: DashboardProps) {
  const summary = useMemo(() => {
    const byStatus = Object.entries(STATUS_LABELS).map(([key, name]) => ({
      name,
      value: demands.filter(d => d.status === key).length,
    }))

    const sectors = demands.reduce<Record<string, number>>((acc, d) => {
      acc[d.sector] = (acc[d.sector] || 0) + 1
      return acc
    }, {})

    const bySector = Object.entries(sectors).map(([name, value]) => ({
      name,
      value,
    }))

    const byPriority = [
      {
        name: 'Alta',
        value: demands.filter(d => d.priority === 'High').length,
      },
      {
        name: 'Normal',
        value: demands.filter(d => d.priority === 'Normal').length,
      },
    ]

    return { byStatus, bySector, byPriority }
  }, [demands])

  const highCount = demands.filter(d => d.priority === 'High').length
  const pendingCount = demands.filter(d => d.status === 'Pending').length
  const completedCount = demands.filter(d => d.status === 'Completed').length

  return (
    <section className="dashboard">
      <div className="dashboard-header">
        <div>
          <h2>Visão geral</h2>
          <p>Acompanhamento das demandas internas</p>
        </div>
      </div>

      <div className="dashboard-stats">
        <article className="stat-card">
          <span>Total de demandas</span>
          <strong>{demands.length}</strong>
        </article>

        <article className="stat-card">
          <span>Prioridade alta</span>
          <strong>{highCount}</strong>
        </article>

        <article className="stat-card">
          <span>Pendentes</span>
          <strong>{pendingCount}</strong>
        </article>

        <article className="stat-card">
          <span>Concluídas</span>
          <strong>{completedCount}</strong>
        </article>
      </div>

      <div className="dashboard-charts">
        <article className="chart-card">
          <h3>Demandas por status</h3>
          <div className="chart-container">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={summary.byStatus}>
                <CartesianGrid strokeDasharray="3 3" vertical={false} />
                <XAxis dataKey="name" />
                <YAxis allowDecimals={false} />
                <Tooltip />
                <Bar dataKey="value" name="Demandas" fill="#6366f1" radius={[5, 5, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </article>

        <article className="chart-card">
          <h3>Demandas por prioridade</h3>
          <div className="chart-container">
            {demands.length > 0 ? (
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={summary.byPriority}
                    dataKey="value"
                    nameKey="name"
                    cx="50%"
                    cy="45%"
                    outerRadius={85}
                    label
                  >
                    {summary.byPriority.map((entry, index) => (
                      <Cell key={entry.name} fill={COLORS[index + 2]} />
                    ))}
                  </Pie>
                  <Tooltip />
                  <Legend />
                </PieChart>
              </ResponsiveContainer>
            ) : (
              <p className="empty-chart">Nenhuma demanda cadastrada</p>
            )}
          </div>
        </article>

        <article className="chart-card chart-wide">
          <h3>Demandas por setor</h3>
          <div className="chart-container">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={summary.bySector} layout="vertical">
                <CartesianGrid strokeDasharray="3 3" horizontal={false} />
                <XAxis type="number" allowDecimals={false} />
                <YAxis type="category" dataKey="name" width={110} />
                <Tooltip />
                <Bar dataKey="value" name="Demandas" fill="#10b981" radius={[0, 5, 5, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </article>
      </div>
    </section>
  )
}
