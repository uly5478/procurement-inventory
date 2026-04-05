import client from './client'
import type { ApiResponse, MonthlyShipmentResult, MonthlyShipmentRecord } from '../types'

export async function getMonthlyShipments(
  productId: number,
  year?: number
): Promise<MonthlyShipmentResult> {
  const params = year ? `?year=${year}` : ''
  const res = await client.get<ApiResponse<MonthlyShipmentResult>>(
    `/api/products/${productId}/monthly-shipments${params}`
  )
  return res.data.data
}

export async function upsertMonthlyShipment(
  productId: number,
  data: { year: number; month: number; quantity: number }
): Promise<MonthlyShipmentRecord> {
  const res = await client.post<ApiResponse<MonthlyShipmentRecord>>(
    `/api/products/${productId}/monthly-shipments`,
    data
  )
  return res.data.data
}

export async function bulkUpsertMonthlyShipments(
  productId: number,
  data: { year: number; monthQuantities: Record<number, number> }
): Promise<MonthlyShipmentRecord[]> {
  const res = await client.put<ApiResponse<MonthlyShipmentRecord[]>>(
    `/api/products/${productId}/monthly-shipments/bulk`,
    data
  )
  return res.data.data
}