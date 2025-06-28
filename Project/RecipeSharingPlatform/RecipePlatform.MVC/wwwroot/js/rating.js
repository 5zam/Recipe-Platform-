/**
 * Fixed Auto-Submit Rating System
 * File: wwwroot/js/rating.js
 */

class AutoSubmitRatingSystem {
    constructor(options = {}) {
        this.recipeId = options.recipeId || 0;
        this.currentRating = options.currentRating || 0;
        this.userRating = options.userRating || 0;
        this.averageRating = options.averageRating || 0;
        this.totalRatings = options.totalRatings || 0;
        this.isReadOnly = options.isReadOnly || false;
        this.isSubmitting = false;

        this.feedbackMessages = {
            0: 'Click on stars to rate this recipe',
            1: '😞 Poor - You didn\'t like this recipe',
            2: '😐 Fair - Recipe was okay',
            3: '🙂 Good - Nice recipe',
            4: '😊 Very Good - Great recipe',
            5: '🤩 Excellent - Amazing recipe!'
        };

        this.init();
    }

    init() {
        this.ensureStarsVisible();
        this.createResetButton(); // CREATE RESET BUTTON ALWAYS
        this.bindEvents();

        if (this.userRating > 0) {
            this.setRating(this.userRating, false);
        }

        console.log('🌟 Auto-Submit Rating System Initialized:', {
            recipeId: this.recipeId,
            userRating: this.userRating,
            averageRating: this.averageRating,
            totalRatings: this.totalRatings
        });
    }

    ensureStarsVisible() {
        const starContainer = document.getElementById('ratingStars');
        if (!starContainer) return;

        const existingStars = starContainer.querySelectorAll('.rating-star');

        if (existingStars.length === 0) {
            this.createStars(starContainer);
        } else {
            existingStars.forEach((star, index) => {
                this.formatStar(star, index + 1);
            });
        }
    }

    createStars(container) {
        container.innerHTML = '';

        for (let i = 1; i <= 5; i++) {
            const star = document.createElement('i');
            this.formatStar(star, i);
            container.appendChild(star);
        }
    }

    formatStar(star, rating) {
        // Remove any existing classes and styles
        star.className = 'fas fa-star rating-star';
        star.setAttribute('data-rating', rating);
        star.setAttribute('tabindex', '0');
        star.setAttribute('role', 'button');
        star.setAttribute('aria-label', `Rate ${rating} out of 5 stars`);

        // FIXED STYLING - NO MORE OVERLAPPING
        star.style.fontSize = '3.8rem';
        star.style.color = '#d1d5db';
        star.style.cursor = 'pointer';
        star.style.transition = 'all 0.3s ease';
        star.style.margin = '0'; // NO MARGINS
        star.style.padding = '10px'; // PADDING FOR EASIER CLICKING
        star.style.userSelect = 'none';
        star.style.display = 'inline-block';
        star.style.borderRadius = '8px';

        // REMOVE BACKGROUND BOX
        star.style.background = 'none';
        star.style.border = 'none';
        star.style.outline = 'none';
        star.style.boxShadow = 'none';
    }

    createResetButton() {
        // Remove existing reset button
        const existingReset = document.getElementById('resetRating');
        if (existingReset) {
            existingReset.remove();
        }

        // Create new reset button
        const resetBtn = document.createElement('button');
        resetBtn.id = 'resetRating';
        resetBtn.type = 'button';
        resetBtn.className = 'reset-rating-btn';
        resetBtn.innerHTML = '<i class="fas fa-undo"></i> Clear Rating';
        resetBtn.title = 'Clear your rating selection';

        // Add to rating container
        const ratingInfo = document.querySelector('.rating-info');
        if (ratingInfo) {
            const buttonContainer = document.createElement('div');
            buttonContainer.className = 'text-center mt-3';
            buttonContainer.appendChild(resetBtn);
            ratingInfo.appendChild(buttonContainer);
        }
    }

    bindEvents() {
        if (this.isReadOnly) return;

        const stars = document.querySelectorAll('.rating-star');
        const resetBtn = document.getElementById('resetRating');

        // Star events - AUTO SUBMIT on click
        stars.forEach(star => {
            star.addEventListener('mouseenter', (e) => this.handleStarHover(e));
            star.addEventListener('mouseleave', () => this.handleStarLeave());
            star.addEventListener('click', (e) => this.handleStarClick(e));

            star.addEventListener('keydown', (e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    this.handleStarClick(e);
                }
            });
        });

        // Reset button event
        if (resetBtn) {
            resetBtn.addEventListener('click', () => this.handleReset());
        }

        // Container leave event
        const starContainer = document.getElementById('ratingStars');
        if (starContainer) {
            starContainer.addEventListener('mouseleave', () => {
                this.updateStarDisplay(this.currentRating);
                this.updateFeedback(this.currentRating);
            });
        }
    }

    handleStarHover(event) {
        if (this.isSubmitting) return;

        const rating = parseInt(event.target.getAttribute('data-rating'));
        this.updateStarDisplay(rating, true);
        this.updateFeedback(rating);
    }

    handleStarLeave() {
        if (this.isSubmitting) return;

        this.updateStarDisplay(this.currentRating);
        this.updateFeedback(this.currentRating);
    }

    async handleStarClick(event) {
        if (this.isSubmitting) return;

        event.preventDefault();
        const rating = parseInt(event.target.getAttribute('data-rating'));

        console.log('⭐ Star clicked:', rating);

        // Set rating and auto-submit
        await this.setRating(rating, true);
        this.animateStar(event.target);
    }

    async handleReset() {
        if (this.isSubmitting) return;

        if (this.userRating === 0) {
            // Just clear selection
            this.setRating(0, false);
            this.showMessage('Rating selection cleared', 'info');
            return;
        }

        // Confirm before removing from database
        if (!confirm('Are you sure you want to remove your rating completely?')) {
            return;
        }

        try {
            this.isSubmitting = true;
            this.showMessage('Removing your rating...', 'info');

            // Call remove rating endpoint
            const success = await this.removeRatingFromServer();

            if (success) {
                // Update local state
                this.userRating = 0;
                this.currentRating = 0;

                // Update UI
                this.setRating(0, false);
                this.updateCurrentRatingText();

                // Fetch updated statistics from server
                await this.refreshRatingStatistics();

                this.showMessage('Your rating has been removed successfully! 🗑️', 'success');

                console.log('✅ Rating removed successfully');
            } else {
                this.showMessage('Error removing rating. Please try again.', 'error');
            }
        } catch (error) {
            console.error('❌ Error removing rating:', error);
            this.showMessage('Error removing rating. Please try again.', 'error');
        } finally {
            this.isSubmitting = false;
        }
    }

    async setRating(rating, shouldSubmit = true) {
        this.currentRating = rating;
        this.updateStarDisplay(rating);
        this.updateFeedback(rating);

        // Update form inputs
        const selectedRatingInput = document.getElementById('selectedRatingInput');
        const selectedRatingDisplay = document.getElementById('selectedRating');

        if (selectedRatingInput) {
            selectedRatingInput.value = rating;
        }
        if (selectedRatingDisplay) {
            selectedRatingDisplay.textContent = rating;
        }

        // Auto-submit if requested
        if (shouldSubmit && rating > 0) {
            await this.submitRatingToServer();
        }

        console.log('⭐ Rating set to:', rating);
    }

    updateStarDisplay(rating, isHover = false) {
        const stars = document.querySelectorAll('.rating-star');

        stars.forEach((star, index) => {
            const starRating = parseInt(star.getAttribute('data-rating'));

            // Remove all state classes
            star.classList.remove('selected', 'temp-hover');

            // Reset to default styling
            star.style.color = '#d1d5db';
            star.style.transform = 'scale(1)';
            star.style.filter = 'none';
            star.style.background = 'none';

            // Apply appropriate state
            if (starRating <= rating) {
                if (isHover) {
                    star.classList.add('temp-hover');
                    star.style.color = '#fbbf24';
                    star.style.transform = 'scale(1.1)';
                    star.style.background = 'rgba(251, 191, 36, 0.1)';
                } else {
                    star.classList.add('selected');
                    star.style.color = '#f59e0b';
                }
            }
        });
    }

    updateFeedback(rating) {
        const ratingFeedback = document.getElementById('ratingFeedback');
        if (ratingFeedback) {
            ratingFeedback.textContent = this.feedbackMessages[rating] || this.feedbackMessages[0];

            // Update feedback colors
            if (rating === 0) {
                ratingFeedback.style.backgroundColor = '#f3f4f6';
                ratingFeedback.style.borderColor = '#d1d5db';
                ratingFeedback.style.color = '#6b7280';
            } else if (rating <= 2) {
                ratingFeedback.style.backgroundColor = '#fef2f2';
                ratingFeedback.style.borderColor = '#fecaca';
                ratingFeedback.style.color = '#dc2626';
            } else if (rating <= 3) {
                ratingFeedback.style.backgroundColor = '#fffbeb';
                ratingFeedback.style.borderColor = '#fed7aa';
                ratingFeedback.style.color = '#d97706';
            } else {
                ratingFeedback.style.backgroundColor = '#f0fdf4';
                ratingFeedback.style.borderColor = '#bbf7d0';
                ratingFeedback.style.color = '#059669';
            }
        }
    }

    animateStar(starElement) {
        starElement.style.transform = 'scale(1.3)';
        starElement.style.filter = 'brightness(1.3) drop-shadow(0 0 15px #f59e0b)';

        setTimeout(() => {
            starElement.style.transform = starElement.classList.contains('selected') ? 'scale(1)' : 'scale(1)';
            starElement.style.filter = 'none';
        }, 400);

        starElement.style.animation = 'starPulse 0.4s ease-in-out';
        setTimeout(() => {
            starElement.style.animation = '';
        }, 400);
    }

    async submitRatingToServer() {
        if (this.currentRating === 0 || this.isSubmitting) return;

        try {
            this.isSubmitting = true;
            this.showMessage('Saving your rating...', 'info');

            const formData = new FormData();
            formData.append('recipeId', this.recipeId);
            formData.append('rateValue', this.currentRating);

            const token = document.querySelector('input[name="__RequestVerificationToken"]');
            if (token) {
                formData.append('__RequestVerificationToken', token.value);
            }

            console.log('📡 Auto-submitting rating:', this.currentRating);

            const response = await fetch('/Rating/AddRating', {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (response.ok) {
                const wasUpdate = this.userRating > 0;
                this.userRating = this.currentRating;

                // Get fresh statistics from server
                await this.refreshRatingStatistics();

                this.updateCurrentRatingText();

                this.showMessage(
                    `Rating ${wasUpdate ? 'updated' : 'submitted'} successfully! ⭐`,
                    'success'
                );

                console.log('✅ Rating auto-submitted successfully');
            } else {
                throw new Error('Server error');
            }
        } catch (error) {
            console.error('❌ Error auto-submitting rating:', error);
            this.showMessage('Error saving rating. Please try again.', 'error');

            // Revert on error
            this.currentRating = this.userRating;
            this.updateStarDisplay(this.currentRating);
        } finally {
            this.isSubmitting = false;
        }
    }

    async removeRatingFromServer() {
        try {
            const formData = new FormData();
            formData.append('recipeId', this.recipeId);

            const token = document.querySelector('input[name="__RequestVerificationToken"]');
            if (token) {
                formData.append('__RequestVerificationToken', token.value);
            }

            const response = await fetch('/Rating/RemoveRating', {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            return response.ok;
        } catch (error) {
            console.error('❌ Error removing rating from server:', error);
            return false;
        }
    }

    async refreshRatingStatistics() {
        try {
            const response = await fetch(`/Rating/GetRatingStats?recipeId=${this.recipeId}`, {
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (response.ok) {
                const data = await response.json();

                if (data.Success) {
                    // Update with real server data
                    this.averageRating = data.AverageRating;
                    this.totalRatings = data.TotalRatings;

                    // Update UI with real data
                    this.updatePageRatingInfo();

                    console.log('📊 Rating statistics refreshed:', {
                        average: this.averageRating,
                        total: this.totalRatings
                    });
                }
            }
        } catch (error) {
            console.error('❌ Error refreshing statistics:', error);
        }
    }

    updateCurrentRatingText() {
        const currentRatingText = document.getElementById('currentRatingText');
        if (currentRatingText) {
            if (this.userRating > 0) {
                currentRatingText.innerHTML = `<span>Your current rating: ${this.userRating}/5 stars</span>`;
            } else {
                currentRatingText.innerHTML = `<span>Click on stars to rate - your rating will be saved automatically</span>`;
            }
        }
    }

    updatePageRatingInfo() {
        // Update average score with REAL data
        const avgScoreEl = document.getElementById('averageScore');
        if (avgScoreEl) {
            avgScoreEl.textContent = this.averageRating.toFixed(1);
        }

        // Update total ratings with REAL data
        const totalRatingsEl = document.getElementById('totalRatings');
        if (totalRatingsEl) {
            totalRatingsEl.textContent = `Based on ${this.totalRatings} ${this.totalRatings === 1 ? 'rating' : 'ratings'}`;
        }

        // Update selected rating display
        const selectedRatingEl = document.getElementById('selectedRating');
        if (selectedRatingEl) {
            selectedRatingEl.textContent = this.currentRating;
        }

        // Update average rating display
        const avgDisplayEl = document.getElementById('avgDisplay');
        if (avgDisplayEl) {
            avgDisplayEl.textContent = this.averageRating.toFixed(1);
        }

        this.updateAverageStars();
    }

    updateAverageStars() {
        const avgStarsEl = document.getElementById('averageStars');
        if (!avgStarsEl) return;

        const stars = avgStarsEl.querySelectorAll('i');
        const rating = this.averageRating;

        stars.forEach((star, index) => {
            star.className = 'fas fa-star';

            if (index < Math.floor(rating)) {
                star.style.color = '#f59e0b';
            } else if (index < rating) {
                star.className = 'fas fa-star-half-alt';
                star.style.color = '#f59e0b';
            } else {
                star.style.color = '#d1d5db';
            }
        });
    }

    showMessage(message, type = 'info') {
        // Create or get message container AT THE TOP
        let messageContainer = document.getElementById('ratingMessages');
        if (!messageContainer) {
            messageContainer = document.createElement('div');
            messageContainer.id = 'ratingMessages';

            // Add to body at the top
            document.body.appendChild(messageContainer);
        }

        const messageEl = document.createElement('div');
        messageEl.className = `alert alert-${type} alert-dismissible fade show`;
        messageEl.innerHTML = `
            <i class="fas fa-${this.getIconForType(type)}"></i> ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        `;

        // Clear existing messages and add new one
        messageContainer.innerHTML = '';
        messageContainer.appendChild(messageEl);

        // Auto-remove after 4 seconds
        setTimeout(() => {
            if (messageEl.parentNode) {
                messageEl.remove();
            }
        }, 4000);
    }

    getIconForType(type) {
        const icons = {
            'success': 'check-circle',
            'error': 'exclamation-circle',
            'warning': 'exclamation-triangle',
            'info': 'info-circle'
        };
        return icons[type] || 'info-circle';
    }

    // Public methods
    getRating() { return this.currentRating; }
    getUserRating() { return this.userRating; }
    getAverageRating() { return this.averageRating; }
    getTotalRatings() { return this.totalRatings; }
    isSubmittingRating() { return this.isSubmitting; }

    setReadOnly(readonly) {
        this.isReadOnly = readonly;

        const stars = document.querySelectorAll('.rating-star');
        const resetBtn = document.getElementById('resetRating');

        stars.forEach(star => {
            if (readonly) {
                star.style.cursor = 'default';
                star.style.pointerEvents = 'none';
            } else {
                star.style.cursor = 'pointer';
                star.style.pointerEvents = 'auto';
            }
        });

        if (resetBtn) {
            resetBtn.style.display = readonly ? 'none' : 'inline-block';
        }
    }

    destroy() {
        // Clean up event listeners
        const stars = document.querySelectorAll('.rating-star');
        const resetBtn = document.getElementById('resetRating');

        stars.forEach(star => {
            star.replaceWith(star.cloneNode(true));
        });

        if (resetBtn) {
            resetBtn.replaceWith(resetBtn.cloneNode(true));
        }
    }
}

// Auto-initialize when page loads
document.addEventListener('DOMContentLoaded', function () {
    console.log('🚀 DOM Content Loaded - Initializing Fixed Auto-Submit Rating System');

    const ratingContainer = document.querySelector('.rating-container');
    const ratingStars = document.getElementById('ratingStars');

    if (ratingContainer || ratingStars) {
        const recipeId = getRecipeId();
        const currentRating = getCurrentUserRating();
        const averageRating = getAverageRating();
        const totalRatings = getTotalRatings();
        const isReadOnly = ratingContainer && ratingContainer.hasAttribute('data-readonly');

        console.log('📊 Rating System Config:', {
            recipeId,
            currentRating,
            averageRating,
            totalRatings,
            isReadOnly
        });

        // Initialize with fixed system
        window.recipeRatingSystem = new AutoSubmitRatingSystem({
            recipeId: recipeId,
            userRating: currentRating,
            currentRating: currentRating,
            averageRating: averageRating,
            totalRatings: totalRatings,
            isReadOnly: isReadOnly
        });

        console.log('✅ Fixed Auto-Submit Rating System initialized successfully');
    } else {
        console.log('ℹ️ No rating container found on this page');
    }
});

// Helper functions to extract data from page
function getRecipeId() {
    // Try hidden input first
    const input = document.querySelector('input[name="recipeId"]');
    if (input) return parseInt(input.value) || 0;

    // Try data attribute
    const container = document.querySelector('[data-recipe-id]');
    if (container) return parseInt(container.getAttribute('data-recipe-id')) || 0;

    // Try URL pattern
    const match = window.location.pathname.match(/\/Recipe\/Details\/(\d+)/);
    if (match) return parseInt(match[1]) || 0;

    console.warn('⚠️ Could not determine recipe ID');
    return 0;
}

function getCurrentUserRating() {
    const input = document.getElementById('selectedRatingInput');
    const value = input ? parseInt(input.value) || 0 : 0;
    console.log('👤 Current user rating:', value);
    return value;
}

function getAverageRating() {
    const element = document.getElementById('averageScore');
    const value = element ? parseFloat(element.textContent) || 0 : 0;
    console.log('📊 Average rating:', value);
    return value;
}

function getTotalRatings() {
    const element = document.getElementById('totalRatings');
    if (!element) return 0;

    const match = element.textContent.match(/(\d+)/);
    const value = match ? parseInt(match[1]) : 0;
    console.log('📈 Total ratings:', value);
    return value;
}

// Utility function for displaying star ratings in other components
function displayStarRating(container, rating, maxStars = 5, size = '1rem') {
    if (!container) return;

    container.innerHTML = '';
    container.style.display = 'flex';
    container.style.gap = '3px';

    for (let i = 1; i <= maxStars; i++) {
        const star = document.createElement('i');
        star.style.fontSize = size;

        if (i <= Math.floor(rating)) {
            star.className = 'fas fa-star';
            star.style.color = '#f59e0b';
        } else if (i <= rating) {
            star.className = 'fas fa-star-half-alt';
            star.style.color = '#f59e0b';
        } else {
            star.className = 'far fa-star';
            star.style.color = '#d1d5db';
        }

        container.appendChild(star);
    }
}

// Debug function for testing
function debugFixedRating() {
    console.log('🔍 Fixed Rating System Debug Info:');
    console.log('- Rating System:', window.recipeRatingSystem);
    console.log('- Stars Count:', document.querySelectorAll('.rating-star').length);
    console.log('- Container:', document.querySelector('.rating-container'));
    console.log('- Reset Button:', document.getElementById('resetRating'));
    console.log('- Messages Container:', document.getElementById('ratingMessages'));

    if (window.recipeRatingSystem) {
        console.log('- Current Rating:', window.recipeRatingSystem.getRating());
        console.log('- User Rating:', window.recipeRatingSystem.getUserRating());
        console.log('- Average Rating:', window.recipeRatingSystem.getAverageRating());
        console.log('- Is Submitting:', window.recipeRatingSystem.isSubmittingRating());
    }

    // Test star spacing
    const stars = document.querySelectorAll('.rating-star');
    stars.forEach((star, index) => {
        console.log(`Star ${index + 1}:`, {
            fontSize: star.style.fontSize,
            margin: star.style.margin,
            padding: star.style.padding,
            gap: window.getComputedStyle(star.parentElement).gap
        });
    });
}

// Make functions available globally
window.displayStarRating = displayStarRating;
window.debugFixedRating = debugFixedRating;

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        AutoSubmitRatingSystem,
        displayStarRating,
        debugFixedRating
    };
}