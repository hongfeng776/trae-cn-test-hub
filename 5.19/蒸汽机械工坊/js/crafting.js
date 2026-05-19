class CraftingManager {
    constructor(game) {
        this.game = game;
        this.recipes = [];
        this.currentCrafting = null;
        this.craftStartTime = 0;
        this.craftEndTime = 0;
        this.recipeElements = {};
        this.isInitialized = false;
    }

    init() {
        console.log('[合成系统] =================================');
        console.log('[合成系统] 开始初始化...');
        
        this.recipes = GameConfig.recipes || [];
        console.log(`[合成系统] 加载了 ${this.recipes.length} 个配方`);
        
        this.recipes.forEach(r => {
            console.log(`[合成系统] 配方: ${r.name}, 材料:`, r.ingredients);
        });
        
        this.renderRecipes();
        this.isInitialized = true;
        
        console.log('[合成系统] 初始化完成!');
        console.log('[合成系统] =================================');
    }

    renderRecipes() {
        const recipeList = document.getElementById('recipe-list');
        if (!recipeList) {
            console.error('[合成系统] 错误: 找不到 recipe-list 元素!');
            return;
        }
        
        recipeList.innerHTML = '';
        
        this.recipes.forEach(recipe => {
            const recipeEl = this.createRecipeElement(recipe);
            recipeList.appendChild(recipeEl);
            this.recipeElements[recipe.id] = recipeEl;
        });
    }

    createRecipeElement(recipe) {
        const el = document.createElement('div');
        el.className = 'recipe-item';
        el.id = `recipe-${recipe.id}`;
        
        const outputResource = Object.keys(recipe.output)[0];
        const outputAmount = recipe.output[outputResource];
        const outputConfig = GameConfig.resources[outputResource] || { icon: '📦', name: outputResource };
        
        let ingredientsHtml = '';
        Object.keys(recipe.ingredients).forEach(ingredientId => {
            const amountNeeded = recipe.ingredients[ingredientId];
            const ingredientConfig = GameConfig.resources[ingredientId] || { icon: '📦', name: ingredientId };
            const currentAmount = this.game.getResource(ingredientId) || 0;
            const isSufficient = currentAmount >= amountNeeded;
            const statusClass = isSufficient ? 'sufficient' : 'insufficient';
            
            ingredientsHtml += `
                <div class="ingredient ${statusClass}" data-ingredient="${ingredientId}">
                    <span class="ingredient-icon">${ingredientConfig.icon}</span>
                    <span class="ingredient-amount">${Math.floor(currentAmount)}/${amountNeeded}</span>
                </div>
            `;
        });
        
        el.innerHTML = `
            <div class="recipe-header">
                <span class="recipe-icon">${recipe.icon}</span>
                <span class="recipe-name">${recipe.name}</span>
            </div>
            <div class="recipe-description">${recipe.description}</div>
            <div class="recipe-ingredients">
                ${ingredientsHtml}
            </div>
            <div class="recipe-output">
                <span class="recipe-output-icon">${outputConfig.icon}</span>
                <span>产出: ${outputConfig.name} ×${outputAmount}</span>
            </div>
            <div class="recipe-footer">
                <span class="recipe-time">⏱️ ${(recipe.craftTime / 1000).toFixed(1)}秒</span>
                <button class="craft-btn" data-recipe="${recipe.id}">合成</button>
            </div>
            <div class="craft-progress">
                <div class="craft-progress-bar" style="width: 0%"></div>
            </div>
        `;
        
        const craftBtn = el.querySelector('.craft-btn');
        if (craftBtn) {
            craftBtn.addEventListener('click', (e) => {
                console.log('[合成系统] 按钮被点击! 配方ID:', recipe.id);
                e.stopPropagation();
                this.startCrafting(recipe.id);
            });
        }
        
        return el;
    }

    canCraft(recipeId) {
        const recipe = this.recipes.find(r => r.id === recipeId);
        if (!recipe) {
            console.log(`[合成系统] canCraft: 找不到配方 ${recipeId}`);
            return false;
        }
        
        if (this.currentCrafting) {
            console.log(`[合成系统] canCraft: 正在合成 ${this.currentCrafting.name}`);
            return false;
        }
        
        for (const [ingredientId, amountNeeded] of Object.entries(recipe.ingredients)) {
            const currentAmount = this.game.getResource(ingredientId) || 0;
            console.log(`[合成系统] canCraft: ${ingredientId} = ${currentAmount}, 需要 ${amountNeeded}`);
            if (currentAmount < amountNeeded) {
                console.log(`[合成系统] canCraft: 材料不足!`);
                return false;
            }
        }
        
        console.log(`[合成系统] canCraft: 可以合成!`);
        return true;
    }

    startCrafting(recipeId) {
        console.log('[合成系统] =================================');
        console.log(`[合成系统] 尝试开始合成: ${recipeId}`);
        
        const recipe = this.recipes.find(r => r.id === recipeId);
        if (!recipe) {
            console.error(`[合成系统] 错误: 未找到配方 ${recipeId}`);
            return false;
        }
        
        if (!this.canCraft(recipeId)) {
            console.warn(`[合成系统] 无法合成: 材料不足或正在合成中`);
            return false;
        }
        
        console.log(`[合成系统] 开始合成: ${recipe.name}`);
        
        for (const [ingredientId, amountNeeded] of Object.entries(recipe.ingredients)) {
            const currentAmount = this.game.getResource(ingredientId);
            const newAmount = currentAmount - amountNeeded;
            this.game.setResource(ingredientId, newAmount);
            console.log(`[合成系统] 消耗材料: ${ingredientId}: ${currentAmount} -> ${newAmount} (-${amountNeeded})`);
        }
        
        this.currentCrafting = recipe;
        this.craftStartTime = performance.now();
        this.craftEndTime = this.craftStartTime + recipe.craftTime;
        
        console.log(`[合成系统] 合成将在 ${recipe.craftTime}ms 后完成`);
        
        this.updateRecipeUI(recipeId, true);
        this.game.updateUI();
        
        console.log('[合成系统] =================================');
        return true;
    }

    update(deltaTime) {
        if (!this.currentCrafting) return;
        
        const now = performance.now();
        const elapsed = now - this.craftStartTime;
        const total = this.craftEndTime - this.craftStartTime;
        const progress = Math.min(elapsed / total, 1);
        
        this.updateProgressBar(this.currentCrafting.id, progress);
        
        if (progress >= 1) {
            this.completeCrafting();
        }
    }

    updateProgressBar(recipeId, progress) {
        const recipeEl = this.recipeElements[recipeId];
        if (!recipeEl) return;
        
        const progressBar = recipeEl.querySelector('.craft-progress-bar');
        if (progressBar) {
            progressBar.style.width = `${progress * 100}%`;
        }
    }

    completeCrafting() {
        const recipe = this.currentCrafting;
        if (!recipe) return;
        
        console.log('[合成系统] =================================');
        console.log(`[合成系统] 合成完成: ${recipe.name}!`);
        
        for (const [outputId, amount] of Object.entries(recipe.output)) {
            const currentAmount = this.game.getResource(outputId) || 0;
            const maxAmount = GameConfig.resources[outputId]?.maxAmount || Infinity;
            const newAmount = Math.min(currentAmount + amount, maxAmount);
            this.game.setResource(outputId, newAmount);
            console.log(`[合成系统] 获得成品: ${outputId}: ${currentAmount} -> ${newAmount} (+${amount})`);
        }
        
        this.showCraftEffect(recipe);
        
        this.updateRecipeUI(recipe.id, false);
        
        this.currentCrafting = null;
        
        this.updateAllRecipeUI();
        this.game.updateUI();
        
        console.log('[合成系统] =================================');
    }

    showCraftEffect(recipe) {
        const effect = document.createElement('div');
        effect.className = 'craft-effect';
        effect.textContent = `${recipe.icon} ${recipe.name} 合成完成!`;
        document.body.appendChild(effect);
        
        setTimeout(() => {
            if (effect.parentNode) {
                effect.parentNode.removeChild(effect);
            }
        }, 1500);
    }

    updateRecipeUI(recipeId, isCrafting) {
        const recipeEl = this.recipeElements[recipeId];
        if (!recipeEl) return;
        
        const craftBtn = recipeEl.querySelector('.craft-btn');
        const progressContainer = recipeEl.querySelector('.craft-progress');
        const progressBar = recipeEl.querySelector('.craft-progress-bar');
        
        if (isCrafting) {
            recipeEl.classList.add('crafting');
            craftBtn.disabled = true;
            craftBtn.textContent = '合成中...';
            progressContainer.style.display = 'block';
            progressBar.style.width = '0%';
        } else {
            recipeEl.classList.remove('crafting');
            craftBtn.disabled = false;
            craftBtn.textContent = '合成';
            progressContainer.style.display = 'none';
        }
    }

    updateAllRecipeUI() {
        this.recipes.forEach(recipe => {
            const recipeEl = this.recipeElements[recipe.id];
            if (!recipeEl) return;
            
            const ingredients = recipeEl.querySelectorAll('.ingredient');
            ingredients.forEach(ingEl => {
                const ingredientId = ingEl.dataset.ingredient;
                const amountNeeded = recipe.ingredients[ingredientId];
                const currentAmount = this.game.getResource(ingredientId) || 0;
                const isSufficient = currentAmount >= amountNeeded;
                
                ingEl.classList.remove('sufficient', 'insufficient');
                ingEl.classList.add(isSufficient ? 'sufficient' : 'insufficient');
                
                const amountEl = ingEl.querySelector('.ingredient-amount');
                if (amountEl) {
                    amountEl.textContent = `${Math.floor(currentAmount)}/${amountNeeded}`;
                }
            });
            
            const craftBtn = recipeEl.querySelector('.craft-btn');
            if (craftBtn && !this.currentCrafting) {
                const canCraft = this.canCraft(recipe.id);
                craftBtn.disabled = !canCraft;
                if (canCraft) {
                    craftBtn.textContent = '合成';
                } else {
                    craftBtn.textContent = '材料不足';
                }
            }
        });
    }
}

if (typeof module !== 'undefined' && module.exports) {
    module.exports = { CraftingManager };
}
