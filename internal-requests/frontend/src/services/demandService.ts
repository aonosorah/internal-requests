
import axios from 'axios'
import type {
  CreateDemandRequest,
  Demand,
  DemandStatus,
} from '../types/demand'

const api = axios.create({
  baseURL: 'https://internal-requests-api.onrender.com/api',
  timeout: 10000,
})

export const demandService = {
  async getAll(): Promise<Demand[]> {
    const response = await api.get<Demand[]>('/demands')
    return response.data
  },

  async create(data: CreateDemandRequest): Promise<Demand> {
    const response = await api.post<Demand>('/demands', data)
    return response.data
  },

  async getById(id: string): Promise<Demand> {
    const response = await api.get<Demand>(`/demands/${id}`)
    return response.data
  },

  async updateStatus(id: string, status: DemandStatus): Promise<Demand> {
    const response = await api.patch<Demand>(
      `/demands/${id}/status`,
      { status }
    )

    return response.data
  },
}
