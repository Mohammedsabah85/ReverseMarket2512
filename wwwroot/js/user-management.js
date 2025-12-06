// User Management JavaScript
// This file handles all user management operations with proper anti-forgery token handling

function getUserAntiForgertyToken() {
    // Try multiple ways to get the token
    let token = null;
    
    // Method 1: Look for token in the current form
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    if (tokenInput) {
        return tokenInput.value;
    }
    
    // Method 2: Look for token in any form on the page
    const forms = document.getElementsByTagName('form');
    for (let form of forms) {
        const formToken = form.querySelector('input[name="__RequestVerificationToken"]');
        if (formToken) {
            return formToken.value;
        }
    }
    
    // Method 3: Generate token from meta tag (if using)
    const metaToken = document.querySelector('meta[name="csrf-token"]');
    if (metaToken) {
        return metaToken.getAttribute('content');
    }
    
    return null;
}

function toggleUserStatus(userId, isCurrentlyActive) {
    const action = isCurrentlyActive ? 'إيقاف' : 'تفعيل';
    const confirmMessage = `هل أنت متأكد من ${action} هذا المستخدم؟`;

    if (!confirm(confirmMessage)) {
        return;
    }

    // Get the anti-forgery token
    const token = getUserAntiForgertyToken();
    if (!token) {
        // If no token found, create a temporary form to generate one
        fetch('/Admin/Users/Details/' + userId)
            .then(() => {
                // Retry after page refresh
                location.reload();
            })
            .catch(() => {
                alert('حدث خطأ في تنفيذ العملية. يرجى تحديث الصفحة والمحاولة مرة أخرى.');
            });
        return;
    }

    // Create and submit form
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = '/Admin/Users/ToggleStatus';
    form.style.display = 'none';

    // Add userId
    const idInput = document.createElement('input');
    idInput.type = 'hidden';
    idInput.name = 'id';
    idInput.value = userId;
    form.appendChild(idInput);

    // Add anti-forgery token
    const tokenInput = document.createElement('input');
    tokenInput.type = 'hidden';
    tokenInput.name = '__RequestVerificationToken';
    tokenInput.value = token;
    form.appendChild(tokenInput);

    document.body.appendChild(form);
    form.submit();
}

function deleteUser(userId, userName) {
    const confirmMessage = `هل أنت متأكد من حذف المستخدم "${userName}"؟\n\nتحذير: هذا الإجراء لا يمكن التراجع عنه!`;

    if (!confirm(confirmMessage)) {
        return;
    }

    // Get the anti-forgery token
    const token = getUserAntiForgertyToken();
    if (!token) {
        alert('حدث خطأ في تنفيذ العملية. يرجى تحديث الصفحة والمحاولة مرة أخرى.');
        return;
    }

    // Create and submit form
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = '/Admin/Users/Delete';
    form.style.display = 'none';

    // Add userId
    const idInput = document.createElement('input');
    idInput.type = 'hidden';
    idInput.name = 'id';
    idInput.value = userId;
    form.appendChild(idInput);

    // Add anti-forgery token
    const tokenInput = document.createElement('input');
    tokenInput.type = 'hidden';
    tokenInput.name = '__RequestVerificationToken';
    tokenInput.value = token;
    form.appendChild(tokenInput);

    document.body.appendChild(form);
    form.submit();
}

// Alternative AJAX-based approach
function toggleUserStatusAjax(userId, isCurrentlyActive) {
    const action = isCurrentlyActive ? 'إيقاف' : 'تفعيل';
    const confirmMessage = `هل أنت متأكد من ${action} هذا المستخدم؟`;

    if (!confirm(confirmMessage)) {
        return;
    }

    const token = getUserAntiForgertyToken();
    if (!token) {
        alert('حدث خطأ في تنفيذ العملية. يرجى تحديث الصفحة والمحاولة مرة أخرى.');
        return;
    }

    // Using AJAX
    fetch('/Admin/Users/ToggleStatus', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token
        },
        body: new URLSearchParams({
            'id': userId,
            '__RequestVerificationToken': token
        })
    })
    .then(response => {
        if (response.ok) {
            location.reload();
        } else {
            throw new Error('Request failed');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('حدث خطأ في تنفيذ العملية');
    });
}

function deleteUserAjax(userId, userName) {
    const confirmMessage = `هل أنت متأكد من حذف المستخدم "${userName}"؟\n\nتحذير: هذا الإجراء لا يمكن التراجع عنه!`;

    if (!confirm(confirmMessage)) {
        return;
    }

    const token = getUserAntiForgertyToken();
    if (!token) {
        alert('حدث خطأ في تنفيذ العملية. يرجى تحديث الصفحة والمحاولة مرة أخرى.');
        return;
    }

    // Using AJAX
    fetch('/Admin/Users/Delete', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token
        },
        body: new URLSearchParams({
            'id': userId,
            '__RequestVerificationToken': token
        })
    })
    .then(response => {
        if (response.ok || response.redirected) {
            window.location.href = '/Admin/Users';
        } else {
            throw new Error('Request failed');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('حدث خطأ في حذف المستخدم');
    });
}

// Export functions if needed
function exportUsers() {
    alert('وظيفة التصدير قيد التطوير');
}

// Refresh statistics
function refreshStats() {
    const refreshBtn = document.querySelector('[onclick="refreshStats()"]');
    if (!refreshBtn) return;
    
    const originalText = refreshBtn.innerHTML;
    refreshBtn.disabled = true;
    refreshBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>جاري التحديث...';

    fetch('/Admin/Users/GetUserStats')
        .then(response => response.json())
        .then(data => {
            // Update stats in the UI
            console.log('Stats updated:', data);
            setTimeout(() => {
                location.reload();
            }, 1000);
        })
        .catch(error => {
            console.error('خطأ في جلب الإحصائيات:', error);
            alert('حدث خطأ في تحديث الإحصائيات');
            refreshBtn.disabled = false;
            refreshBtn.innerHTML = originalText;
        });
}

// Initialize on document ready
document.addEventListener('DOMContentLoaded', function() {
    // Auto-refresh stats every 5 minutes
    if (document.querySelector('[onclick="refreshStats()"]')) {
        setInterval(refreshStats, 300000);
    }
    
    // Initialize search improvements
    const searchInput = document.getElementById('search');
    const userTypeSelect = document.getElementById('userType');
    const isActiveSelect = document.getElementById('isActive');

    // Save search in localStorage
    if (searchInput) {
        searchInput.addEventListener('input', function() {
            localStorage.setItem('userSearch', this.value);
        });

        // Restore saved search
        const savedSearch = localStorage.getItem('userSearch');
        if (savedSearch && !searchInput.value) {
            searchInput.value = savedSearch;
        }
    }

    // Highlight active filters
    [userTypeSelect, isActiveSelect].forEach(select => {
        if (select) {
            select.addEventListener('change', function() {
                if (this.value) {
                    this.style.borderColor = '#007bff';
                } else {
                    this.style.borderColor = '';
                }
            });
            
            // Highlight if already has value
            if (select.value) {
                select.style.borderColor = '#007bff';
            }
        }
    });

    // Improve accessibility
    document.querySelectorAll('.btn-group .btn').forEach(btn => {
        btn.addEventListener('focus', function() {
            this.style.boxShadow = '0 0 0 0.2rem rgba(0,123,255,.25)';
        });

        btn.addEventListener('blur', function() {
            this.style.boxShadow = '';
        });
    });
});

// Debug helper - log user ID when clicking on actions
if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
    window.addEventListener('click', function(e) {
        if (e.target.closest('[onclick*="toggleUserStatus"]') || 
            e.target.closest('[onclick*="deleteUser"]')) {
            const onclick = e.target.closest('[onclick*="UserStatus"]') || 
                          e.target.closest('[onclick*="deleteUser"]');
            if (onclick) {
                console.log('Action clicked:', onclick.getAttribute('onclick'));
            }
        }
    });
}
