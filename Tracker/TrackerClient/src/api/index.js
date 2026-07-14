const API_BASE_URL = import.meta.env.VITE_API_URL || '';

async function request(url, options = {}) {
  const config = {
    headers: { 'Content-Type': 'application/json' },
    ...options,
  };

  if (config.body && typeof config.body === 'object') {
    config.body = JSON.stringify(config.body);
  }

  const response = await fetch(`${API_BASE_URL}${url}`, config);

  const text = await response.text();
  const data = text ? JSON.parse(text) : null;

  if (!response.ok) {
    const errorMessage = data?.message || `Ошибка ${response.status}`;
    throw new Error(errorMessage);
  }

  return data;
}

export const api = {
  // Категории
  getCategories: () =>
    request('/categories?includeInactive=true'),
  createCategory: (data) =>
    request('/categories', { method: 'POST', body: data }),
  updateCategory: (id, data) =>
    request(`/categories/${id}`, { method: 'PUT', body: data }),
  deleteCategory: (id) =>
    request(`/categories/${id}`, { method: 'DELETE' }),

  // Статьи расходов
  getExpenseItems: () =>
    request('/expenseitems?includeInactive=true'),
  createExpenseItem: (data) =>
    request('/expenseitems', { method: 'POST', body: data }),
  updateExpenseItem: (id, data) =>
    request(`/expenseitems/${id}`, { method: 'PUT', body: data }),
  deleteExpenseItem: (id) =>
    request(`/expenseitems/${id}`, { method: 'DELETE' }),

  // Транзакции
  getTransactions: (params = {}) => {
    const query = new URLSearchParams();
    if (params.date) query.append('date', params.date);
    if (params.month) query.append('month', params.month);
    if (params.year) query.append('year', params.year);
    const qs = query.toString();
    return request(`/transactions${qs ? '?' + qs : ''}`);
  },
  createTransaction: (data) =>
    request('/transactions', { method: 'POST', body: data }),
  updateTransaction: (id, data) =>
    request(`/transactions/${id}`, { method: 'PUT', body: data }),
  deleteTransaction: (id) =>
    request(`/transactions/${id}`, { method: 'DELETE' }),
  getDailySummary: (date) =>
    request(`/transactions/daily-summary?date=${date}`),
};