import { api } from '@/api/index.js'

export default {
    name: 'App',
    data() {
        return {
            selectedDate: new Date().toISOString().split('T')[0],
            selectedMonth: new Date().toISOString().slice(0, 7),
            filterDay: new Date().toISOString().split('T')[0],
            filterPeriod: 'all',

            transactions: [],
            categories: [],
            expenseItems: [],

            newTransaction: {
                date: new Date().toISOString().split('T')[0],
                amount: null,
                expenseItemId: '',
                comment: ''
            },
            newCategory: {
                name: '',
                monthlyBudget: 0
            },
            newExpenseItem: {
                name: '',
                categoryId: ''
            },

            // Для редактирования
            editingCategory: null,
            editingExpenseItem: null,

            dailyTotal: 0,
            dailyLimitWarning: ''
        }
    },

    computed: {
        activeCategories() {
            return this.categories.filter(c => c.isActive)
        },
        activeExpenseItems() {
            return this.expenseItems.filter(e => e.isActive)
        },
        stickerClass() {
            if (this.dailyTotal < 500) return 'sticker-green'
            if (this.dailyTotal <= 2000) return 'sticker-yellow'
            return 'sticker-red'
        },
        stickerText() {
            if (this.dailyTotal < 500) return 'Экономный день!'
            if (this.dailyTotal <= 2000) return 'В пределах нормы'
            return 'Затратный день'
        }
    },

    mounted() {
        this.loadAllData()
    },

    methods: {
        async loadAllData() {
            await Promise.all([
                this.loadCategories(),
                this.loadExpenseItems()
            ])
            await this.loadTransactions()
            await this.updateDailyTotal()
        },

        async loadCategories() {
            try {
                this.categories = await api.getCategories()
            } catch (error) {
                console.error('Ошибка загрузки категорий:', error)
            }
        },

        async loadExpenseItems() {
            try {
                this.expenseItems = await api.getExpenseItems()
            } catch (error) {
                console.error('Ошибка загрузки статей:', error)
            }
        },

        async loadTransactions() {
            try {
                const params = {}
                if (this.filterPeriod === 'day') {
                    params.date = this.filterDay
                } else if (this.filterPeriod === 'month') {
                    params.month = this.selectedMonth
                }
                this.transactions = await api.getTransactions(params)
            } catch (error) {
                console.error('Ошибка загрузки транзакций:', error)
                this.transactions = []
            }
        },

        async updateDailyTotal() {
            try {
                const summary = await api.getDailySummary(this.selectedDate)
                this.dailyTotal = summary.totalAmount || 0
                if (this.dailyTotal > 1000000) {
                    this.dailyLimitWarning = 'Достигнут дневной лимит в 1 000 000 ₽'
                } else if (this.dailyTotal > 900000) {
                    this.dailyLimitWarning = `Осталось ${(1000000 - this.dailyTotal).toLocaleString()} ₽ до лимита`
                } else {
                    this.dailyLimitWarning = ''
                }
            } catch (error) {
                console.error('Ошибка загрузки дневной сводки:', error)
                this.dailyTotal = 0
            }
        },

        onDateChange() {
            this.updateDailyTotal()
            if (this.filterPeriod === 'day') {
                this.loadTransactions()
            }
        },

        setFilterAll() {
            this.filterPeriod = 'all'
            this.loadTransactions()
        },
        setFilterDay() {
            this.filterPeriod = 'day'
            this.filterDay = this.selectedDate
            this.loadTransactions()
        },
        setFilterMonth() {
            this.filterPeriod = 'month'
            this.loadTransactions()
        },

        // ТРАНЗАКЦИИ 
        async addTransaction() {
            try {
                await api.createTransaction({ ...this.newTransaction })
                this.newTransaction = {
                    date: this.selectedDate,
                    amount: null,
                    expenseItemId: '',
                    comment: ''
                }
                await this.loadTransactions()
                await this.updateDailyTotal()
            } catch (error) {
                alert('Ошибка: ' + (error.message || 'Не удалось добавить транзакцию'))
            }
        },

        async deleteTransaction(id) {
            if (!confirm('Удалить транзакцию?')) return
            try {
                await api.deleteTransaction(id)
                await this.loadTransactions()
                await this.updateDailyTotal()
            } catch (error) {
                alert(error.message || 'Ошибка при удалении транзакции')
            }
        },

        // КАТЕГОРИИ 
        async addCategory() {
            try {
                await api.createCategory({
                    name: this.newCategory.name,
                    monthlyBudget: this.newCategory.monthlyBudget,
                    isActive: true
                })
                this.newCategory = { name: '', monthlyBudget: 0 }
                await this.loadCategories()
            } catch (error) {
                alert('Ошибка: ' + error.message)
            }
        },

        startEditCategory(category) {
            this.editingCategory = { ...category }
        },

        cancelEditCategory() {
            this.editingCategory = null
        },

        async saveCategory() {
            try {
                await api.updateCategory(this.editingCategory.id, {
                    name: this.editingCategory.name,
                    monthlyBudget: this.editingCategory.monthlyBudget,
                    isActive: this.editingCategory.isActive
                })
                this.editingCategory = null
                await this.loadCategories()
                await this.loadTransactions()
                await this.updateDailyTotal()
            } catch (error) {
                alert('Ошибка: ' + error.message)
            }
        },

        async toggleCategory(category) {
            try {
                await api.updateCategory(category.id, {
                    name: category.name,
                    monthlyBudget: category.monthlyBudget,
                    isActive: !category.isActive
                })
                // Меняем локально
                category.isActive = !category.isActive
                await this.loadTransactions()
                await this.updateDailyTotal()
            } catch (error) {
                alert('Ошибка: ' + error.message)
                await this.loadCategories()
            }
        },


        async deleteCategory(id) {
            if (!confirm('Удалить категорию?')) return
            try {
                await api.deleteCategory(id)
                await this.loadAllData()
            } catch (error) {
                alert(error.message || 'Ошибка при удалении категории')
            }
        },

        // СТАТЬИ РАСХОДОВ 
        async addExpenseItem() {
            try {
                await api.createExpenseItem({
                    name: this.newExpenseItem.name,
                    categoryId: this.newExpenseItem.categoryId,
                    isActive: true
                })
                this.newExpenseItem = { name: '', categoryId: '' }
                await this.loadExpenseItems()
            } catch (error) {
                alert('Ошибка: ' + error.message)
            }
        },

        startEditExpenseItem(item) {
            this.editingExpenseItem = { ...item }
        },

        cancelEditExpenseItem() {
            this.editingExpenseItem = null
        },

        async saveExpenseItem() {
            try {
                await api.updateExpenseItem(this.editingExpenseItem.id, {
                    name: this.editingExpenseItem.name,
                    categoryId: this.editingExpenseItem.categoryId,
                    isActive: this.editingExpenseItem.isActive
                })
                this.editingExpenseItem = null
                await this.loadExpenseItems()
                await this.loadTransactions()
                await this.updateDailyTotal()
            } catch (error) {
                alert('Ошибка: ' + error.message)
            }
        },

        async toggleExpenseItem(item) {
            try {
                await api.updateExpenseItem(item.id, {
                    name: item.name,
                    categoryId: item.categoryId,
                    isActive: !item.isActive
                })
                // Меняем локально
                item.isActive = !item.isActive
                await this.loadTransactions()
                await this.updateDailyTotal()
            } catch (error) {
                alert('Ошибка: ' + error.message)
                await this.loadExpenseItems()
            }
        },

        async deleteExpenseItem(id) {
            if (!confirm('Удалить статью расхода?')) return
            try {
                await api.deleteExpenseItem(id)
                await this.loadAllData()
            } catch (error) {
                alert(error.message || 'Ошибка при удалении статьи')
            }
        },

        formatDate(dateString) {
            if (!dateString) return ''
            const d = new Date(dateString)
            return d.toLocaleDateString('ru-RU', {
                day: 'numeric',
                month: 'long',
                year: 'numeric'
            })
        }
    }
}