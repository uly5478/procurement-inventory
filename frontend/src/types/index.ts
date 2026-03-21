// ─── Shared API response wrappers ────────────────────────────────────────────

export interface ApiResponse<T> {
  data: T
  success: boolean
  message?: string
  requestId?: string
}

export interface WarningResponse {
  warning: string
  requireConfirmation: boolean
}

// ─── Product ──────────────────────────────────────────────────────────────────

export interface Product {
  id: number
  productCode: string
  name: string
  unit: string
  isActive: boolean
  createdAt: string
  updatedAt: string
}

// ─── Supplier & pricing ───────────────────────────────────────────────────────

export interface Supplier {
  id: number
  name: string
  contactInfo: string
}

export interface ProductSupplierPrice {
  id: number
  productId: number
  supplierId: number
  supplierName: string
  unitPrice: number
  currency: string
  minOrderQty: number
  leadTimeDays: number
  effectiveDate: string
  isCurrent: boolean
}

// ─── Purchase order ───────────────────────────────────────────────────────────

export type PurchaseOrderStatus = 'Pending' | 'Confirmed' | 'Received' | 'Cancelled'

export interface PurchaseOrderItem {
  id: number
  purchaseOrderId: number
  productId: number
  productName: string
  quantity: number
  unitPrice: number
  subtotal: number
}

export interface PurchaseOrder {
  id: number
  orderNumber: string
  supplierId: number
  supplierName: string
  status: PurchaseOrderStatus
  totalAmount: number
  orderDate: string
  createdAt: string
  createdBy: string
  items: PurchaseOrderItem[]
}

// ─── Inventory ────────────────────────────────────────────────────────────────

export interface InventoryRecord {
  id: number
  productId: number
  productName: string
  currentStock: number
  updatedAt: string
}

export type TransactionType = 'StockIn' | 'StockOut'

export interface StockTransaction {
  id: number
  productId: number
  transactionType: TransactionType
  quantity: number
  stockBefore: number
  stockAfter: number
  purchaseOrderId?: number
  transactionDate: string
  operatorAccount: string
  createdAt: string
  remark?: string
}

// ─── Procurement suggestion & settings ───────────────────────────────────────

export interface ProcurementSuggestion {
  id: number
  productId: number
  productName: string
  productCode: string
  currentStock: number
  sixMonthAvgShipment: number
  turnoverMonths: number
  systemSuggestedQty: number
  manualOverrideQty?: number
  isManualOverride: boolean
  dataInsufficient: boolean
  availableMonths?: number
  calculatedAt: string
}

export interface ProcurementSettings {
  id: number
  defaultTurnoverMonths: number
  updatedBy: string
  updatedAt: string
}

// ─── Demand forecast ─────────────────────────────────────────────────────────

export interface DemandForecast {
  id: number
  productId: number
  forecastMonth: number
  forecastYear: number
  forecastQty: number
  confidenceLower: number
  confidenceUpper: number
  generatedAt: string
}
