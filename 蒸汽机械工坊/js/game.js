class Game {
    constructor() {
        this.machineManager = new MachineManager();
        this.craftingManager = null;
        this.resources = {};
        this.lastUpdateTime = 0;
        this.isRunning = false;
        this.totalProducedEver = 0;
        this.uiUpdateTimer = 0;
        this.productionAccumulator = {};
    }
    
    init() {
        console.log('[游戏] 蒸汽机械工坊启动中...');
        
        Object.keys(GameConfig.resources).forEach(resource => {
            this.resources[resource] = GameConfig.resources[resource].initialAmount || 0;
            this.productionAccumulator[resource] = 0;
        });
        
        this.machineManager.init();
        
        this.machineManager.addProductionCallback((machine, production) => {
            this.onMachineProduction(machine, production);
        });
        
        this.craftingManager = new CraftingManager(this);
        this.craftingManager.init();
        
        this.bindEvents();
        this.startGameLoop();
        this.startUIUpdates();
        this.updateUI();
        this.isRunning = true;
        
        console.log('[游戏] 蒸汽机械工坊初始化完成!');
        console.log('[游戏] 初始资源:', this.resources);
    }
    
    bindEvents() {
        const btnStartAll = document.getElementById('btn-start-all');
        const btnStopAll = document.getElementById('btn-stop-all');
        const btnAddCoal = document.getElementById('btn-add-coal');
        
        if (btnStartAll) {
            btnStartAll.addEventListener('click', () => {
                this.machineManager.startAllMachines();
                this.updateUI();
            });
        }
        
        if (btnStopAll) {
            btnStopAll.addEventListener('click', () => {
                this.machineManager.stopAllMachines();
                this.updateUI();
            });
        }
        
        if (btnAddCoal) {
            btnAddCoal.addEventListener('click', () => {
                this.addCoal(GameConfig.resources.coal.perClick);
            });
        }
    }
    
    startGameLoop() {
        this.lastUpdateTime = performance.now();
        requestAnimationFrame((timestamp) => this.gameLoop(timestamp));
    }
    
    gameLoop(timestamp) {
        const deltaTime = timestamp - this.lastUpdateTime;
        this.lastUpdateTime = timestamp;
        
        this.update(deltaTime);
        
        requestAnimationFrame((newTimestamp) => this.gameLoop(newTimestamp));
    }
    
    update(deltaTime) {
        this.machineManager.update(deltaTime);
        
        if (this.craftingManager) {
            this.craftingManager.update(deltaTime);
        }
        
        const coalConsumption = this.machineManager.getTotalCoalConsumption();
        
        if (coalConsumption > 0) {
            const coalNeeded = coalConsumption * (deltaTime / 1000);
            if (this.resources.coal >= coalNeeded) {
                this.resources.coal -= coalNeeded;
            }
        }
        
        const activeFurnaces = this.machineManager.getMachinesByType('coal-furnace').filter(m => m.status === 'active');
        if (activeFurnaces.length > 0) {
            const steamProduction = activeFurnaces.length * GameConfig.resources.steam.pressurePerMachine;
            this.resources.steam = Math.min(
                this.resources.steam + steamProduction * (deltaTime / 1000),
                GameConfig.resources.steam.maxAmount
            );
        }
        
        this.uiUpdateTimer += deltaTime;
        if (this.uiUpdateTimer >= 100) {
            this.updateUI();
            this.uiUpdateTimer = 0;
        }
    }
    
    onMachineProduction(machine, production) {
        Object.keys(production).forEach(resource => {
            if (resource !== 'steam' && resource !== 'coal') {
                const maxAmount = GameConfig.resources[resource]?.maxAmount || Infinity;
                this.resources[resource] = Math.min(
                    this.resources[resource] + production[resource],
                    maxAmount
                );
                this.totalProducedEver += production[resource];
            }
        });
        
        console.log(`[游戏生产] ${machine.id} 生产:`, production, '当前资源:', this.resources);
    }
    
    startUIUpdates() {
        console.log('[游戏] UI自动刷新已启动');
    }
    
    stopUIUpdates() {
    }
    
    updateUI() {
        const steamPressureElement = document.getElementById('steam-pressure');
        const coalLevelElement = document.getElementById('coal-level');
        
        if (steamPressureElement) {
            steamPressureElement.textContent = `蒸汽压力: ${Math.round(this.resources.steam)} psi`;
        }
        
        if (coalLevelElement) {
            coalLevelElement.textContent = `煤炭储量: ${Math.round(this.resources.coal)}`;
        }
        
        this.updateResourcePanel();
        
        if (this.craftingManager) {
            this.craftingManager.updateAllRecipeUI();
        }
    }
    
    updateResourcePanel() {
        const productionPerSecond = this.machineManager.getProductionPerSecond();
        const activeMachinesCount = this.machineManager.getActiveMachines().length;
        
        Object.keys(GameConfig.resources).forEach(resource => {
            const amountElement = document.getElementById(`res-${resource}`);
            const rateElement = document.getElementById(`rate-${resource}`);
            
            if (amountElement) {
                amountElement.textContent = Math.round(this.resources[resource] || 0);
            }
            
            if (rateElement) {
                const rate = productionPerSecond[resource] || 0;
                if (rate > 0) {
                    rateElement.textContent = `+${rate}/s`;
                    rateElement.className = 'resource-rate';
                } else if (rate < 0) {
                    rateElement.textContent = `${rate}/s`;
                    rateElement.className = 'resource-rate negative';
                } else {
                    rateElement.textContent = '+0/s';
                    rateElement.className = 'resource-rate';
                }
            }
        });
        
        const activeMachinesElement = document.getElementById('active-machines');
        if (activeMachinesElement) {
            activeMachinesElement.textContent = activeMachinesCount;
        }
        
        const totalProducedElement = document.getElementById('total-produced');
        if (totalProducedElement) {
            totalProducedElement.textContent = Math.round(this.totalProducedEver);
        }
    }
    
    addCoal(amount) {
        this.resources.coal = Math.min(
            this.resources.coal + amount,
            GameConfig.resources.coal.maxAmount
        );
        console.log(`[游戏] 添加煤炭 +${amount}, 当前: ${Math.round(this.resources.coal)}`);
        this.updateUI();
    }
    
    getResource(name) {
        return this.resources[name] || 0;
    }
    
    setResource(name, amount) {
        if (this.resources.hasOwnProperty(name)) {
            this.resources[name] = Math.max(0, amount);
            this.updateUI();
        }
    }
    
    getMachineManager() {
        return this.machineManager;
    }
    
    destroy() {
        this.stopUIUpdates();
        this.machineManager.stopSteamParticles();
        this.isRunning = false;
        console.log('[游戏] 蒸汽机械工坊已关闭');
    }
}

let gameInstance = null;

document.addEventListener('DOMContentLoaded', () => {
    gameInstance = new Game();
    gameInstance.init();
});

if (typeof module !== 'undefined' && module.exports) {
    module.exports = { Game, gameInstance };
}
