export type DemandStatus = 'Pending' | 'InProgress' | 'Completed'
export type DemandPriority = 'Normal' | 'High'

export interface Demand {
  id: string
  title: string
  description: string
  requestDate: string
  sector: string
  status: DemandStatus
  priority: DemandPriority
}

export interface CreateDemandRequest {
  title: string
  description: string
  requestDate: string
  sector: string
  status: DemandStatus
}
