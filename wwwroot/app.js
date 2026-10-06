const API = '/api/v1';
const TOKEN_KEY = 'ta_token';

const $ = (sel) => document.querySelector(sel);
const loginView = $('#login-view');
const dashboardView = $('#dashboard-view');
const loginForm = $('#login-form');
const loginError = $('#login-error');
const teamSelect = $('#team-select');
const datePicker = $('#date-picker');
const membersGrid = $('#members-grid');
const oncallList = $('#oncall-list');
const jiraPanel = $('#jira-panel');
const dateLabel = $('#selected-date-label');

function getToken() {
  return localStorage.getItem(TOKEN_KEY);
}

function setToken(token) {
  if (token) localStorage.setItem(TOKEN_KEY, token);
  else localStorage.removeItem(TOKEN_KEY);
}

async function api(path, options = {}) {
  const headers = { 'Content-Type': 'application/json', ...options.headers };
  const token = getToken();
  if (token) headers.Authorization = `Bearer ${token}`;

  const res = await fetch(`${API}${path}`, { ...options, headers });
  if (res.status === 401) {
    setToken(null);
    showLogin();
    throw new Error('Сессия истекла');
  }
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.detail || err.title || `Ошибка ${res.status}`);
  }
  if (res.status === 204) return null;
  return res.json();
}

function showLogin() {
  loginView.classList.remove('hidden');
  dashboardView.classList.add('hidden');
}

function showDashboard() {
  loginView.classList.add('hidden');
  dashboardView.classList.remove('hidden');
}

function formatDate(d) {
  return d.toLocaleDateString('ru-RU', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' });
}

function presenceBadge(status) {
  const cls = status === 'Working' ? 'working' : 'away';
  const label = status === 'Working' ? 'На работе' : 'Не на связи';
  return `<span class="badge ${cls}">${label}</span>`;
}

function availabilityBadge(isAvailable, reason) {
  if (isAvailable) return '<span class="badge available">Доступен</span>';
  return `<span class="badge unavailable">${reason || 'Недоступен'}</span>`;
}

function renderMembers(data) {
  if (!data.members?.length) {
    membersGrid.innerHTML = '<p class="empty">Нет участников</p>';
    return;
  }

  membersGrid.innerHTML = data.members.map((m) => `
    <article class="member-card">
      <div class="name">${escapeHtml(m.displayName)}</div>
      <div class="meta">${escapeHtml(m.timeZoneId)}</div>
      ${presenceBadge(m.presenceStatus)}
      ${availabilityBadge(m.isAvailable, m.unavailableReason)}
      ${m.replacementName ? `<div class="meta">Замена: ${escapeHtml(m.replacementName)}</div>` : ''}
    </article>
  `).join('');
}

function renderOnCall(shifts) {
  if (!shifts?.length) {
    oncallList.innerHTML = '<p class="empty">Нет запланированных смен</p>';
    return;
  }

  oncallList.innerHTML = shifts.map((s) => {
    const start = new Date(s.startUtc).toLocaleString('ru-RU');
    const end = new Date(s.endUtc).toLocaleString('ru-RU');
    const jira = s.jiraIssueUrl
      ? `<a href="${escapeHtml(s.jiraIssueUrl)}" target="_blank" rel="noopener">${escapeHtml(s.jiraIssueKey)}</a>`
      : '';
    return `<div class="list-item"><strong>${escapeHtml(s.memberName)}</strong><br>${start} — ${end}${jira ? `<br>Jira: ${jira}` : ''}</div>`;
  }).join('');
}

function renderJira(data) {
  if (!data.available?.length && !data.unavailable?.length) {
    jiraPanel.innerHTML = '<p class="empty">Нет данных</p>';
    return;
  }

  const avail = (data.available || []).map((m) =>
    `<div class="list-item"><strong>${escapeHtml(m.displayName)}</strong> — можно назначить (${escapeHtml(m.presenceStatus)})</div>`
  ).join('');

  const unavail = (data.unavailable || []).map((m) =>
    `<div class="list-item"><strong>${escapeHtml(m.displayName)}</strong> — ${escapeHtml(m.unavailableReason || 'недоступен')}${m.replacementName ? `, замена: ${escapeHtml(m.replacementName)}` : ''}</div>`
  ).join('');

  jiraPanel.innerHTML = avail + unavail;
}

function escapeHtml(str) {
  return String(str ?? '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}

async function loadTeams() {
  const teams = await api('/teams');
  teamSelect.innerHTML = teams.map((t) =>
    `<option value="${t.id}">${escapeHtml(t.name)} (${t.memberCount})</option>`
  ).join('');
}

async function refreshDashboard() {
  const teamId = teamSelect.value;
  const date = datePicker.value;
  dateLabel.textContent = formatDate(new Date(date + 'T12:00:00'));

  const [availability, oncall, jira] = await Promise.all([
    api(`/teams/${teamId}/availability?date=${date}`),
    api(`/teams/${teamId}/on-call`),
    api(`/integrations/jira/teams/${teamId}/assignable?date=${date}`)
  ]);

  renderMembers(availability);
  renderOnCall(oncall);
  renderJira(jira);
}

loginForm.addEventListener('submit', async (e) => {
  e.preventDefault();
  loginError.classList.add('hidden');
  try {
    const result = await api('/auth/login', {
      method: 'POST',
      body: JSON.stringify({
        email: $('#email').value,
        password: $('#password').value
      })
    });
    setToken(result.accessToken);
    showDashboard();
    await initDashboard();
  } catch (err) {
    loginError.textContent = err.message;
    loginError.classList.remove('hidden');
  }
});

$('#logout-btn').addEventListener('click', () => {
  setToken(null);
  showLogin();
});

teamSelect.addEventListener('change', () => refreshDashboard().catch(console.error));
datePicker.addEventListener('change', () => refreshDashboard().catch(console.error));

async function initDashboard() {
  const today = new Date().toISOString().slice(0, 10);
  datePicker.value = today;
  await loadTeams();
  await refreshDashboard();
}

if (getToken()) {
  showDashboard();
  initDashboard().catch(() => showLogin());
} else {
  showLogin();
}
