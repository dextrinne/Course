<template>
  <div id="app">
    <header class="header">
      <h1>Приложение для учёта личных финансов и бюджетирования</h1>
      <p class="subtitle" lang="ru">Пользователь ведет учет своих личных расходов.
        Каждая расходная операция относится к определенной категории трат (например, "Продукты", "Транспорт",
        "Развлечения").
        У пользователя может быть множество категорий. В один день можно совершить несколько трат из разных категорий.
      </p>
    </header>

    <main class="content">
      <!-- Цветовой стикер дня -->
      <section class="block">
        <h2>Статус дня</h2>
        <div class="date-picker">
          <input type="date" v-model="selectedDate" @change="onDateChange" class="date-input" />
        </div>
        <div :class="['sticker', stickerClass]">
          <div class="sticker-text">{{ stickerText }}</div>
          <div class="sticker-amount">{{ dailyTotal.toLocaleString() }} ₽</div>
        </div>
        <div v-if="dailyLimitWarning" class="warning"> {{ dailyLimitWarning }}</div>
      </section>

      <!-- Добавление транзакции -->
      <section class="block">
        <h2>Новая транзакция</h2>
        <form @submit.prevent="addTransaction" class="form">
          <div class="form-group">
            <label>Дата</label>
            <input type="date" v-model="newTransaction.date" required />
          </div>
          <div class="form-group">
            <label>Сумма (₽)</label>
            <input type="number" v-model.number="newTransaction.amount" required min="0.01" step="0.01"
              placeholder="0.00" />
          </div>
          <div class="form-group">
            <label>Статья расхода</label>
            <select v-model="newTransaction.expenseItemId" required>
              <option value="">Выберите статью</option>
              <option v-for="item in activeExpenseItems" :key="item.id" :value="item.id">
                {{ item.name }} ({{ item.category?.name }})
              </option>
            </select>
          </div>
          <div class="form-group">
            <label>Комментарий</label>
            <input type="text" v-model="newTransaction.comment" placeholder="Например: Обед в столовой" />
          </div>
          <button type="submit" class="btn btn-primary btn-full">Добавить</button>
        </form>
      </section>

      <!-- Список транзакций -->
      <section class="block">
        <h2>Транзакции</h2>

        <!-- Фильтры -->
        <div class="filter-controls">
          <div class="filter-buttons">
            <button @click="setFilterAll" :class="['btn', filterPeriod === 'all' ? 'btn-primary' : 'btn-outline']">
              Всё время
            </button>
            <button @click="setFilterDay" :class="['btn', filterPeriod === 'day' ? 'btn-primary' : 'btn-outline']">
              За день
            </button>
            <button @click="setFilterMonth" :class="['btn', filterPeriod === 'month' ? 'btn-primary' : 'btn-outline']">
              За месяц
            </button>
          </div>

          <!-- Календарик для выбора дня -->
          <div v-if="filterPeriod === 'day'" class="date-selector">
            <input type="date" v-model="filterDay" @change="loadTransactions" class="date-input" />
          </div>

          <!-- Календарик для выбора месяца -->
          <div v-if="filterPeriod === 'month'" class="month-selector">
            <input type="month" v-model="selectedMonth" @change="loadTransactions" class="month-input" />
          </div>
        </div>

        <div class="list-container">
          <div v-if="transactions.length === 0" class="empty-state">
            Нет транзакций за выбранный период
          </div>
          <div v-for="transaction in transactions" :key="transaction.id" class="list-item">
            <div class="list-item-info">
              <div class="list-item-date">
                {{ formatDate(transaction.date) }}
              </div>
              <div class="list-item-details">
                <span class="list-item-amount">
                  {{ transaction.amount.toLocaleString() }} ₽
                </span>
                <span class="list-item-category">
                  {{ transaction.expenseItemName || "Удалена" }}
                </span>
              </div>
              <div v-if="transaction.comment" class="list-item-comment">
                {{ transaction.comment }}
              </div>
            </div>
            <button @click="deleteTransaction(transaction.id)" class="btn btn-danger btn-sm" title="Удалить">
              ✕
            </button>
          </div>
        </div>
      </section>

      <!-- Управление статьями расходов -->
      <section class="block">
        <h2>Статьи расходов</h2>
        <div class="list-container">
          <div v-for="item in expenseItems" :key="item.id" :class="['list-item', { inactive: !item.isActive }]">

            <!-- РЕЖИМ РЕДАКТИРОВАНИЯ -->
            <div v-if="editingExpenseItem?.id === item.id" class="list-item-info edit-mode">
              <input v-model="editingExpenseItem.name" class="form-input" />
              <select v-model="editingExpenseItem.categoryId" class="form-input">
                <option v-for="cat in categories" :key="cat.id" :value="cat.id">{{ cat.name }}</option>
              </select>
              <div class="edit-buttons">
                <button @click="saveExpenseItem" class="btn btn-primary btn-sm">✓</button>
                <button @click="cancelEditExpenseItem" class="btn btn-danger btn-sm">✕</button>
              </div>
            </div>

            <!-- РЕЖИМ ПРОСМОТРА -->
            <div v-else class="list-item-info">
              <div class="list-item-name">{{ item.name }}</div>
              <div class="list-item-category-name">Категория: {{ item.category?.name || "Не указана" }}</div>
              <div class="list-item-status">
                <label class="toggle">
                  <input type="checkbox" :checked="item.isActive" @change="toggleExpenseItem(item)" />
                  <span class="toggle-slider"></span>
                  {{ item.isActive ? "Активна" : "Неактивна" }}
                </label>
              </div>
            </div>

            <div class="list-item-actions">
              <button v-if="item.isActive" @click="startEditExpenseItem(item)" class="btn btn-primary btn-sm"
                title="Изменить">✎</button>
              <button @click="deleteExpenseItem(item.id)" class="btn btn-danger btn-sm" title="Удалить">✕</button>
            </div>
          </div>
        </div>

        <form @submit.prevent="addExpenseItem" class="add-form">
          <h3>Добавить статью</h3>
          <div class="form-row">
            <input v-model="newExpenseItem.name" placeholder="Название" required class="form-input" />
            <select v-model="newExpenseItem.categoryId" required class="form-input">
              <option value="">Категория</option>
              <option v-for="cat in activeCategories" :key="cat.id" :value="cat.id">{{ cat.name }}</option>
            </select>
            <button type="submit" class="btn btn-primary">Добавить</button>
          </div>
        </form>
      </section>

      <!-- Управление категориями -->
      <section class="block">
        <h2>Категории</h2>
        <div class="list-container">
          <div v-for="category in categories" :key="category.id"
            :class="['list-item', { inactive: !category.isActive }]">

            <!-- РЕЖИМ РЕДАКТИРОВАНИЯ -->
            <div v-if="editingCategory?.id === category.id" class="list-item-info edit-mode">
              <input v-model="editingCategory.name" class="form-input" />
              <input v-model.number="editingCategory.monthlyBudget" type="number" class="form-input" />
              <div class="edit-buttons">
                <button @click="saveCategory" class="btn btn-primary btn-sm">✓</button>
                <button @click="cancelEditCategory" class="btn btn-danger btn-sm">✕</button>
              </div>
            </div>

            <!-- РЕЖИМ ПРОСМОТРА -->
            <div v-else class="list-item-info">
              <div class="list-item-name">{{ category.name }}</div>
              <div class="list-item-budget">Бюджет: {{ category.monthlyBudget.toLocaleString() }} ₽</div>
              <div class="list-item-status">
                <label class="toggle">
                  <input type="checkbox" :checked="category.isActive" @change="toggleCategory(category)" />
                  <span class="toggle-slider"></span>
                  {{ category.isActive ? "Активна" : "Неактивна" }}
                </label>
              </div>
            </div>

            <div class="list-item-actions">
              <button v-if="category.isActive" @click="startEditCategory(category)" class="btn btn-primary btn-sm"
                title="Изменить">✎</button>
              <button @click="deleteCategory(category.id)" class="btn btn-danger btn-sm" title="Удалить">✕</button>
            </div>
          </div>
        </div>

        <form @submit.prevent="addCategory" class="add-form">
          <h3>Добавить категорию</h3>
          <div class="form-row">
            <input v-model="newCategory.name" placeholder="Название" required class="form-input" />
            <input v-model.number="newCategory.monthlyBudget" type="number" placeholder="Бюджет" required min="0"
              class="form-input" />
            <button type="submit" class="btn btn-primary">Добавить</button>
          </div>
        </form>
      </section>
    </main>
  </div>
</template>

<script>
import AppLogic from "./App.js";
export default AppLogic;
</script>

<style>
@import "./App.css";
</style>
