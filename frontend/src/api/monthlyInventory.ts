import client from './client'
import type { ApiResponse, MonthlyInventory } from '../types'

export async function getMonthlyInventory(
  productId: number,
  months: number = 12
): Promise<MonthlyInventory[]> {
  const res = await client.get<ApiResponse<MonthlyInventory[]>>(
    `/api/products/${productId}/monthly-inventory?months=${months}`
  )
  return res.data.data
}

export async function recordMonthlySnapshot(
  productId: number,
  data: {
    orderQty: number
    stockQty: number
    stockAmount: number
    monthlyShipmentAmount: number
  }
): Promise<MonthlyInventory> {
  const res = await client.post<ApiResponse<MonthlyInventory>>(
    `/api/products/${productId}/monthly-inventory`,
    data
  )
  return res.data.data
}