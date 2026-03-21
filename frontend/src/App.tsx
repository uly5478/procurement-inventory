import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import MainLayout from './layouts/MainLayout'
import ProductsPage from './pages/products/ProductsPage'
import SupplierPricePage from './pages/suppliers/SupplierPricePage'
import SuggestionsPage from './pages/procurement/SuggestionsPage'
import OrdersPage from './pages/procurement/OrdersPage'
import InventoryPage from './pages/inventory/InventoryPage'
import StockInPage from './pages/inventory/StockInPage'
import StockOutPage from './pages/inventory/StockOutPage'
import InventoryHistoryPage from './pages/inventory/InventoryHistoryPage'
import ForecastPage from './pages/forecast/ForecastPage'

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<MainLayout />}>
          <Route index element={<Navigate to="/products" replace />} />
          <Route path="products" element={<ProductsPage />} />
          <Route path="products/:id/suppliers" element={<SupplierPricePage />} />
          <Route path="procurement/suggestions" element={<SuggestionsPage />} />
          <Route path="procurement/orders" element={<OrdersPage />} />
          <Route path="inventory" element={<InventoryPage />} />
          <Route path="inventory/stock-in" element={<StockInPage />} />
          <Route path="inventory/stock-out" element={<StockOutPage />} />
          <Route path="inventory/:productId/history" element={<InventoryHistoryPage />} />
          <Route path="forecast" element={<ForecastPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}
