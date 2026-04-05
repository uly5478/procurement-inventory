import client from './client'
import type { ApiResponse, PurchaseOrder } from '../types'

export interface CreatePurchaseOrderItemDto {
  productId: number
  quantity: number
  unitPrice: number
}

export interface CreatePurchaseOrderDto {
  supplierId: number
  items: CreatePurchaseOrderItemDto[]
}

export interface PurchaseOrderQueryParams {
  startDate?: string
  endDate?: string
  supplierName?: string
  status?: string
}

export async function getPurchaseOrders(params?: PurchaseOrderQueryParams) {
  const res = await client.get<ApiResponse<PurchaseOrder[]>>('/purchase-orders', { params })
  return res.data.data
}

export async function getPurchaseOrder(id: number) {
  const res = await client.get<ApiResponse<PurchaseOrder>>(`/purchase-orders/${id}`)
  return res.data.data
}

export async function createPurchaseOrder(dto: CreatePurchaseOrderDto) {
  const res = await client.post<ApiResponse<PurchaseOrder>>('/purchase-orders', dto)
  return res.data.data
}

export interface PurchaseOrderStats {
  totalOrders: number
  pendingOrders: number
  confirmedOrders: number
  receivedOrders: number
  cancelledOrders: number
  totalAmount: number
  monthlyStats: { label: string; orderCount: number; totalAmount: number }[]
  supplierStats: { supplierName: string; orderCount: number; totalAmount: number }[]
}

export async function updateOrderStatus(id: number, status: string) {
  const res = await client.patch<ApiResponse<PurchaseOrder>>(`/purchase-orders/${id}/status`, { status })
  return res.data.data
}

export async function getOrderStats() {
  const res = await client.get<ApiResponse<PurchaseOrderStats>>('/purchase-orders/stats')
  return res.data.data
}

export function getOrderExportUrl(id: number) {
  return `${client.defaults.baseURL}/purchase-orders/${id}/export`
}

export async function downloadSupplierExcel(supplierName?: string): Promise<{ blob: Blob; filename: string }> {
  const params = supplierName ? { supplierName } : {}
  const res = await client.get('/purchase-orders/export-by-supplier', {
    params,
    responseType: 'blob',
  })
  const filename = supplierName
    ? `発注書_${supplierName}_${new Date().toISOString().slice(0, 10)}.xlsx`
    : `発注書_全仕入先_${new Date().toISOString().slice(0, 10)}.xlsx`
  return { blob: res.data as Blob, filename }
}

export async function downloadOrderExcel(id: number, orderNumber: string): Promise<{ blob: Blob; filename: string }> {
  const res = await client.get(`/purchase-orders/${id}/export`, { responseType: 'blob' })
  return { blob: res.data as Blob, filename: `発注書_${orderNumber}.xlsx` }
}