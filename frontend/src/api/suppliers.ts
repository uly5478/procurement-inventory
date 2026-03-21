import client from './client'
import type { ApiResponse, ProductSupplierPrice } from '../types'

export interface SupplierPriceListResult {
  items: ProductSupplierPrice[]
  warning?: string
  requireConfirmation: boolean
}

export interface CreateSupplierPriceDto {
  supplierName: string
  unitPrice: number
  currency: string
  minOrderQty: number
  leadTimeDays: number
  forceCreate?: boolean
}

export interface UpdateSupplierPriceDto {
  unitPrice: number
  currency: string
  minOrderQty: number
  leadTimeDays: number
}

export async function getProductSuppliers(productId: number) {
  const res = await client.get<ApiResponse<SupplierPriceListResult>>(
    `/products/${productId}/suppliers`,
  )
  return res.data.data
}

export async function addSupplierPrice(productId: number, dto: CreateSupplierPriceDto) {
  const res = await client.post<ApiResponse<SupplierPriceListResult>>(
    `/products/${productId}/suppliers`,
    dto,
  )
  return res.data.data
}

export async function updateSupplierPrice(priceId: number, dto: UpdateSupplierPriceDto) {
  const res = await client.put<ApiResponse<ProductSupplierPrice>>(
    `/suppliers/${priceId}`,
    dto,
  )
  return res.data.data
}
