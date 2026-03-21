import client from './client'
import type { ApiResponse } from '../types'

export interface DemandForecastDto {
  productId: number
  productCode: string
  productName: string
  forecastMonth: number
  forecastYear: number
  forecastQty: number
  confidenceLower: number
  confidenceUpper: number
  generatedAt: string
}

export interface MonthlyShipmentDto {
  year: number
  month: number
  totalShipped: number
}

export interface ProductForecastDetail {
  productId: number
  productCode: string
  productName: string
  historicalShipments: MonthlyShipmentDto[]
  forecast?: DemandForecastDto
  errorMessage?: string
}

export async function getAllForecasts() {
  const res = await client.get<ApiResponse<DemandForecastDto[]>>('/forecast')
  return res.data.data
}

export async function getProductForecast(productId: number) {
  const res = await client.get<ApiResponse<ProductForecastDetail>>(`/forecast/${productId}`)
  return res.data.data
}
