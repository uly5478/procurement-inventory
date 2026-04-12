import client from './client'
import type { ApiResponse } from '../types'

export interface InventoryOverview {
  productId: number
  productCode: string
  productName: string
  unit: string
  sixMonthAvgShipment: number
  stockStatus: 'Normal' | 'Low'
  updatedAt: string
  // 倉庫欄位
  warehouse89: number
  warehouse81: number
  warehouseInspection: number
  warehouse4th: number
  totalWarehouseStock: number
  unallocatedQty: number
  shippedQty: number
  safetyStock: number
  turnoverMonths: number
  leadTimeMonths: number
  // ForecastPage 計算用
  currentStock: number
  recommendedLeadTimeDays?: number
  moq: number
  boxQty: number
  // 半年分の月次発注提案
  monthlyOrderSuggestions?: MonthlyOrderSuggestion[]
}

export interface MonthlyOrderSuggestion {
  label: string
  year: number
  month: number
  suggestedQty: number
  estimatedStock: number
}

export interface StockInDto {
  productId: number
  quantity: number
  transactionDate: string
  purchaseOrderId?: number
  remark?: string
}

export interface StockOutDto {
  productId: number
  quantity: number
  transactionDate: string
  remark?: string
  forceConfirm?: boolean
}

export interface StockTransactionResult {
  transactionId?: number
  productId: number
  productName: string
  transactionType: string
  quantity: number
  stockBefore: number
  stockAfter: number
  transactionDate: string
  operatorAccount: string
  remark?: string
  warning?: string
  requireConfirmation?: boolean
}

export interface MonthlyShipment {
  year: number
  month: number
  totalShipped: number
}

export interface StockTransactionHistory {
  id: number
  productId: number
  transactionType: string
  quantity: number
  stockBefore: number
  stockAfter: number
  purchaseOrderId?: number
  transactionDate: string
  operatorAccount: string
  createdAt: string
  remark?: string
}

export async function getInventoryOverview() {
  const res = await client.get<ApiResponse<InventoryOverview[]>>('/inventory')
  return res.data.data
}

export async function stockIn(dto: StockInDto) {
  const res = await client.post<ApiResponse<StockTransactionResult>>('/inventory/stock-in', dto)
  return res.data.data
}

export async function stockOut(dto: StockOutDto) {
  const res = await client.post<ApiResponse<StockTransactionResult>>('/inventory/stock-out', dto)
  return res.data.data
}

export async function getTransactionHistory(
  productId: number,
  startDate?: string,
  endDate?: string,
) {
  const params: Record<string, string> = {}
  if (startDate) params.startDate = startDate
  if (endDate) params.endDate = endDate
  const res = await client.get<ApiResponse<StockTransactionHistory[]>>(
    `/inventory/${productId}/history`,
    { params },
  )
  return res.data.data
}

export async function getMonthlySummary(productId: number, months = 6) {
  const res = await client.get<ApiResponse<MonthlyShipment[]>>(
    `/inventory/${productId}/monthly-summary`,
    { params: { months } },
  )
  return res.data.data
}

export function getExportUrl() {
  return `${client.defaults.baseURL}/inventory/export`
}
