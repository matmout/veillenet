// AI Summary Modal - Fetch and display AI summaries in a popup
(function() {
    // Create modal HTML structure
    function createModalHTML() {
        const modalHTML = `
            <div id="ai-modal-overlay" class="ai-modal-overlay" role="dialog" aria-labelledby="ai-modal-title" aria-modal="true">
                <div class="ai-modal">
                    <div class="ai-modal-header">
                        <h2 id="ai-modal-title" class="ai-modal-title">
                            <i class="bi bi-stars"></i>
                            <span>AI Summary</span>
                        </h2>
                        <button class="ai-modal-close" aria-label="Close modal" id="ai-modal-close-btn">
                            <i class="bi bi-x-lg"></i>
                        </button>
                    </div>
                    <div class="ai-modal-body" id="ai-modal-body">
                        <div class="ai-modal-loading">
                            <i class="bi bi-arrow-repeat"></i>
                            <p>Loading AI summary...</p>
                        </div>
                    </div>
                    <div class="ai-modal-meta" id="ai-modal-meta" style="display: none;">
                        <!-- Metadata will be inserted here -->
                    </div>
                </div>
            </div>
        `;
        
        // Append to body
        document.body.insertAdjacentHTML('beforeend', modalHTML);
    }

    // Initialize modal on page load
    function initModal() {
        // Create modal if it doesn't exist
        if (!document.getElementById('ai-modal-overlay')) {
            createModalHTML();
        }

        const overlay = document.getElementById('ai-modal-overlay');
        const closeBtn = document.getElementById('ai-modal-close-btn');
        const modalBody = document.getElementById('ai-modal-body');
        const modalMeta = document.getElementById('ai-modal-meta');

        // Close modal function
        function closeModal() {
            overlay.classList.remove('active');
            document.body.style.overflow = ''; // Re-enable scroll
        }

        // Close on close button click
        closeBtn.addEventListener('click', closeModal);

        // Close on overlay click (outside modal)
        overlay.addEventListener('click', function(e) {
            if (e.target === overlay) {
                closeModal();
            }
        });

        // Close on Escape key
        document.addEventListener('keydown', function(e) {
            if (e.key === 'Escape' && overlay.classList.contains('active')) {
                closeModal();
            }
        });

        // Prevent modal content clicks from closing
        document.querySelector('.ai-modal').addEventListener('click', function(e) {
            e.stopPropagation();
        });

        // Add click handlers to all AI summary links
        document.addEventListener('click', function(e) {
            // Find if clicked element or parent is an AI summary link
            const link = e.target.closest('a[href*="/AiSummary"]');
            if (link) {
                e.preventDefault();
                const url = new URL(link.href);
                const articleUrl = url.searchParams.get('url');
                
                if (articleUrl) {
                    openModal(articleUrl);
                }
            }
        });

        // Open modal and fetch AI summary
        function openModal(articleUrl) {
            // Show modal with loading state
            overlay.classList.add('active');
            document.body.style.overflow = 'hidden'; // Disable scroll
            
            modalBody.innerHTML = `
                <div class="ai-modal-loading">
                    <i class="bi bi-arrow-repeat"></i>
                    <p>Loading AI summary...</p>
                </div>
            `;
            modalMeta.style.display = 'none';

            // Fetch AI summary
            fetch(`/api/ai-summary?url=${encodeURIComponent(articleUrl)}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Failed to fetch AI summary');
                    }
                    return response.json();
                })
                .then(data => {
                    if (data.success) {
                        // Display summary
                        modalBody.innerHTML = data.summary || '<p class="text-muted">No summary available.</p>';
                        
                        // Display metadata
                        if (data.title || data.source || data.publishedDate || data.url) {
                            const metaHTML = `
                                ${data.title ? `<div class="ai-modal-meta-item"><i class="bi bi-file-text"></i> ${escapeHtml(data.title)}</div>` : ''}
                                ${data.source ? `<div class="ai-modal-meta-item"><i class="bi bi-tag"></i> ${escapeHtml(data.source)}</div>` : ''}
                                ${data.publishedDate ? `<div class="ai-modal-meta-item"><i class="bi bi-calendar"></i> ${new Date(data.publishedDate).toLocaleDateString()}</div>` : ''}
                                ${data.url ? `<div class="ai-modal-meta-item"><i class="bi bi-link-45deg"></i> <a href="${escapeHtml(data.url)}" target="_blank" rel="noopener" class="ai-modal-meta-link">View original</a></div>` : ''}
                            `;
                            modalMeta.innerHTML = metaHTML;
                            modalMeta.style.display = 'flex';
                        }
                    } else {
                        throw new Error(data.message || 'Unknown error');
                    }
                })
                .catch(error => {
                    console.error('Error fetching AI summary:', error);
                    modalBody.innerHTML = `
                        <div class="ai-modal-error">
                            <i class="bi bi-exclamation-triangle"></i>
                            <p><strong>Error loading AI summary</strong></p>
                            <p>${escapeHtml(error.message)}</p>
                        </div>
                    `;
                });
        }

        // HTML escape utility
        function escapeHtml(text) {
            const div = document.createElement('div');
            div.textContent = text;
            return div.innerHTML;
        }
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initModal);
    } else {
        initModal();
    }
})();
