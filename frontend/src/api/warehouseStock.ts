import client from './client'
import type { ApiResponse, WarehouseStock } from '../types'

export async function getWarehouseStock(productId: number): Promise<WarehouseStock> {
  const res = await client.get<ApiResponse<WarehouseStock>>(
    `/api/products/${productId}/warehouse-stock`
  )
  return res.data.data
}

export async function updateWarehouseStock(
  productId: number,
  data: {
    warehouse89: number
    warehouse81: number
    warehouseInspection: number
    warehouse4th: number
    unallocatedQty: number
    shippedQty: number
  }
): Promise<WarehouseStock> {
  const res = await client.put<ApiResponse<WarehouseStock>>(
    `/api/products/${productId}/warehouse-stock`,
    data
  )
  return res.data.data
}