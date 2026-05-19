class ProductionCoroutine {
    constructor(machine, interval, callback) {
        this.machine = machine;
        this.interval = interval;
        this.callback = callback;
        this.isRunning = false;
        this.accumulatedTime = 0;
        this.totalProduction = {};
        this.productionThisSecond = {};
        this.lastSecondTime = 0;
    }
    
    start() {
        if (this.isRunning) return;
        this.isRunning = true;
        this.accumulatedTime = 0;
        this.lastSecondTime = 0;
        this.productionThisSecond = {};
        console.log(`[生产协程] ${this.machine.id} 已启动, 生产间隔: ${this.interval}ms`);
    }
    
    stop() {
        if (!this.isRunning) return;
        this.isRunning = false;
        console.log(`[生产协程] ${this.machine.id} 已停止`);
    }
    
    update(deltaTime) {
        if (!this.isRunning) return null;
        
        this.accumulatedTime += deltaTime;
        this.lastSecondTime += deltaTime;
        
        if (this.accumulatedTime >= this.interval) {
            this.accumulatedTime -= this.interval;
            const production = this.callback(this.machine);
            
            if (production) {
                Object.keys(production).forEach(resource => {
                    this.totalProduction[resource] = (this.totalProduction[resource] || 0) + production[resource];
                    this.productionThisSecond[resource] = (this.productionThisSecond[resource] || 0) + production[resource];
                });
            }
            
            return production;
        }
        
        if (this.lastSecondTime >= 1000) {
            this.lastSecondTime -= 1000;
            this.productionThisSecond = {};
        }
        
        return null;
    }
    
    getProductionThisSecond() {
        return { ...this.productionThisSecond };
    }
    
    getTotalProduction() {
        return { ...this.totalProduction };
    }
}

class Machine {
    constructor(id, type, element) {
        this.id = id;
        this.type = type;
        this.element = element;
        this.status = 'idle';
        this.config = GameConfig.machines[type] || {};
        this.statusElement = element.querySelector('.machine-status');
        this.productionCoroutine = null;
        this.totalProduced = {};
        this.productionCallback = null;
        
        this.init();
    }
    
    init() {
        this.setStatus('idle');
        
        this.productionCoroutine = new ProductionCoroutine(
            this,
            GameConfig.production.interval,
            (machine) => this.produce()
        );
        
        if (this.config.produces) {
            Object.keys(this.config.produces).forEach(resource => {
                this.totalProduced[resource] = 0;
            });
        }
        
        if (this.config.idleAnimation && this.config.idleAnimation.enabled) {
            this.startIdleAnimation();
        }
        
        this.element.addEventListener('click', () => this.toggle());
    }
    
    setStatus(status) {
        this.status = status;
        this.element.classList.remove('idle', 'active', 'stopped');
        this.element.classList.add(status);
        
        if (this.statusElement) {
            const statusText = {
                idle: '状态: 闲置',
                active: '状态: 运行中',
                stopped: '状态: 停止',
            };
            this.statusElement.textContent = statusText[status] || '状态: 未知';
        }
    }
    
    startIdleAnimation() {
        if (this.status !== 'idle') return;
        
        const animationType = this.config.idleAnimation?.type || 'bob';
        
        switch (animationType) {
            case 'bob':
                this.element.classList.add('idle');
                break;
            case 'pulse':
                this.startPulseAnimation();
                break;
            case 'gear-spin-slow':
                this.startSlowGearSpin();
                break;
        }
    }
    
    stopIdleAnimation() {
        this.element.classList.remove('idle');
    }
    
    startPulseAnimation() {
        this.element.style.animation = `pulse ${this.config.idleAnimation.duration}ms ease-in-out infinite`;
    }
    
    startSlowGearSpin() {
        const gears = this.element.querySelectorAll('.gear');
        gears.forEach((gear, index) => {
            const direction = index % 2 === 0 ? '' : '-reverse';
            gear.style.animation = `gear-rotate${direction} ${this.config.idleAnimation.duration}ms linear infinite`;
        });
    }
    
    start() {
        if (this.status === 'active') return;
        
        this.stopIdleAnimation();
        this.setStatus('active');
        this.startActiveAnimation();
        this.productionCoroutine.start();
    }
    
    stop() {
        if (this.status === 'stopped' || this.status === 'idle') return;
        
        this.stopActiveAnimation();
        this.productionCoroutine.stop();
        this.setStatus('idle');
        this.startIdleAnimation();
    }
    
    produce() {
        if (this.status !== 'active' || !this.config.produces) return null;
        
        const production = {};
        Object.keys(this.config.produces).forEach(resource => {
            const amount = this.config.produces[resource];
            production[resource] = amount;
            this.totalProduced[resource] = (this.totalProduced[resource] || 0) + amount;
        });
        
        console.log(`[生产] ${this.id} 产出:`, production);
        
        if (this.productionCallback) {
            this.productionCallback(this, production);
        }
        
        this.showProductionEffect(production);
        return production;
    }
    
    showProductionEffect(production) {
        const resourceNames = Object.keys(production);
        if (resourceNames.length === 0) return;
        
        const effect = document.createElement('div');
        effect.className = 'production-effect';
        
        const displayResource = resourceNames[0];
        const amount = production[displayResource];
        const icons = {
            gears: '⚙️',
            plates: '🔩',
            pipes: '🛠️',
            steam: '💨',
            coal: '🔥'
        };
        effect.textContent = `${icons[displayResource] || '+'}+${amount}`;
        
        effect.style.left = `${Math.random() * 60 + 20}%`;
        effect.style.top = `${Math.random() * 30 + 10}%`;
        
        this.element.appendChild(effect);
        
        setTimeout(() => {
            if (effect.parentNode) {
                effect.parentNode.removeChild(effect);
            }
        }, 1200);
    }
    
    setProductionCallback(callback) {
        this.productionCallback = callback;
    }
    
    getProductionStats() {
        return { ...this.totalProduced };
    }
    
    getProductionPerSecond() {
        if (this.status !== 'active' || !this.config.produces) return {};
        return { ...this.config.produces };
    }
    
    toggle() {
        if (this.status === 'active') {
            this.stop();
        } else {
            this.start();
        }
    }
    
    startActiveAnimation() {
        if (this.type === 'steam-engine') {
            this.startSteamEngineAnimation();
        } else if (this.type === 'press-machine') {
            this.startPressMachineAnimation();
        } else if (this.type === 'coal-furnace') {
            this.startFurnaceAnimation();
        }
    }
    
    stopActiveAnimation() {
        const animatedElements = this.element.querySelectorAll('.piston, .flywheel, .gear, .press-plate, .press-lever');
        animatedElements.forEach(el => {
            el.style.animation = '';
        });
    }
    
    startSteamEngineAnimation() {
        const piston = this.element.querySelector('.piston');
        const flywheel = this.element.querySelector('.flywheel');
        const gears = this.element.querySelectorAll('.gear');
        
        if (piston) {
            piston.style.animation = `piston-move ${this.config.activeAnimation.pistonSpeed}ms ease-in-out infinite`;
        }
        
        if (flywheel) {
            flywheel.style.animation = `flywheel-spin ${this.config.activeAnimation.flywheelSpeed}ms linear infinite`;
        }
        
        gears.forEach((gear, index) => {
            if (gear.classList.contains('gear-large')) {
                gear.style.animation = `gear-rotate ${this.config.activeAnimation.gearSpeed}ms linear infinite`;
            } else if (gear.classList.contains('gear-medium')) {
                gear.style.animation = `gear-rotate-reverse ${this.config.activeAnimation.gearSpeed * 0.75}ms linear infinite`;
            } else if (gear.classList.contains('gear-small')) {
                gear.style.animation = `gear-rotate ${this.config.activeAnimation.gearSpeed * 0.5}ms linear infinite`;
            }
        });
    }
    
    startPressMachineAnimation() {
        const pressPlate = this.element.querySelector('.press-plate');
        const pressLever = this.element.querySelector('.press-lever');
        const gears = this.element.querySelectorAll('.gear');
        
        if (pressPlate) {
            pressPlate.style.animation = `press-move ${this.config.activeAnimation.pressSpeed}ms ease-in-out infinite`;
        }
        
        if (pressLever) {
            pressLever.style.animation = `lever-move ${this.config.activeAnimation.leverSpeed}ms ease-in-out infinite`;
        }
        
        gears.forEach((gear, index) => {
            const speed = index === 0 ? 2000 : 1500;
            const direction = index % 2 === 0 ? '' : '-reverse';
            gear.style.animation = `gear-rotate${direction} ${speed}ms linear infinite`;
        });
    }
    
    startFurnaceAnimation() {
        this.element.classList.add('active');
    }
    
    update(deltaTime) {
        const production = this.productionCoroutine.update(deltaTime);
        
        return {
            baseProduction: this.status === 'active' ? (this.config.baseProduction || 0) : 0,
            production: production,
        };
    }
    
    getProduction() {
        return this.status === 'active' ? (this.config.baseProduction || 0) : 0;
    }
    
    getCoalConsumption() {
        return this.status === 'active' ? (this.config.coalConsumption || 0) : 0;
    }
    
    getSteamConsumption() {
        return this.status === 'active' ? (this.config.steamConsumption || 0) : 0;
    }
}

class MachineManager {
    constructor() {
        this.machines = [];
        this.particleContainer = null;
        this.particleInterval = null;
        this.totalProduction = {};
        this.productionPerSecond = {};
        this.productionHistory = [];
        this.productionCallbacks = [];
        this.lastUpdateTime = 0;
    }
    
    init() {
        console.log('[机械管理器] 初始化开始');
        
        GameConfig.initialMachines.forEach(machineConfig => {
            const element = document.getElementById(machineConfig.id);
            if (element) {
                const machine = new Machine(
                    machineConfig.id,
                    machineConfig.type,
                    element
                );
                machine.setProductionCallback((m, prod) => this.onProduction(m, prod));
                this.machines.push(machine);
                console.log(`[机械管理器] 机械 ${machineConfig.id} 已注册`);
            } else {
                console.warn(`[机械管理器] 未找到机械元素 ${machineConfig.id}`);
            }
        });
        
        Object.keys(GameConfig.resources).forEach(resource => {
            this.totalProduction[resource] = 0;
            this.productionPerSecond[resource] = 0;
        });
        
        this.particleContainer = document.querySelector('.steam-particles');
        this.startSteamParticles();
        
        console.log(`[机械管理器] 初始化完成, 共 ${this.machines.length} 台机械`);
    }
    
    onProduction(machine, production) {
        Object.keys(production).forEach(resource => {
            this.totalProduction[resource] = (this.totalProduction[resource] || 0) + production[resource];
        });
        
        this.productionHistory.push({
            time: Date.now(),
            production: production,
            machine: machine.id,
        });
        
        if (this.productionHistory.length > 100) {
            this.productionHistory.shift();
        }
        
        this.productionCallbacks.forEach(callback => callback(machine, production));
    }
    
    addProductionCallback(callback) {
        this.productionCallbacks.push(callback);
    }
    
    removeProductionCallback(callback) {
        const index = this.productionCallbacks.indexOf(callback);
        if (index > -1) {
            this.productionCallbacks.splice(index, 1);
        }
    }
    
    getMachine(id) {
        return this.machines.find(m => m.id === id);
    }
    
    getMachinesByType(type) {
        return this.machines.filter(m => m.type === type);
    }
    
    startAllMachines() {
        console.log('[机械管理器] 启动全部机械');
        this.machines.forEach(machine => machine.start());
    }
    
    stopAllMachines() {
        console.log('[机械管理器] 停止全部机械');
        this.machines.forEach(machine => machine.stop());
    }
    
    getActiveMachines() {
        return this.machines.filter(m => m.status === 'active');
    }
    
    getTotalCoalConsumption() {
        return this.machines.reduce((total, machine) => total + machine.getCoalConsumption(), 0);
    }
    
    getTotalSteamConsumption() {
        return this.machines.reduce((total, machine) => total + machine.getSteamConsumption(), 0);
    }
    
    startSteamParticles() {
        if (!GameConfig.particles.steam.enabled) return;
        
        this.particleInterval = setInterval(() => {
            this.spawnSteamParticle();
        }, GameConfig.particles.steam.spawnInterval);
    }
    
    spawnSteamParticle() {
        if (!this.particleContainer) return;
        
        const activeCount = this.getActiveMachines().length;
        if (activeCount === 0) return;
        
        const currentParticles = this.particleContainer.querySelectorAll('.steam-particle').length;
        if (currentParticles >= GameConfig.particles.steam.maxParticles) return;
        
        const particle = document.createElement('div');
        particle.className = 'steam-particle';
        
        const activeMachines = this.getActiveMachines();
        const randomMachine = activeMachines[Math.floor(Math.random() * activeMachines.length)];
        const rect = randomMachine.element.getBoundingClientRect();
        const containerRect = this.particleContainer.getBoundingClientRect();
        
        particle.style.left = `${rect.left - containerRect.left + rect.width / 2 + (Math.random() - 0.5) * 40}px`;
        particle.style.bottom = `${containerRect.bottom - rect.top + Math.random() * 20}px`;
        particle.style.animationDuration = `${GameConfig.particles.steam.riseDuration}ms`;
        
        this.particleContainer.appendChild(particle);
        
        setTimeout(() => {
            if (particle.parentNode) {
                particle.parentNode.removeChild(particle);
            }
        }, GameConfig.particles.steam.riseDuration);
    }
    
    stopSteamParticles() {
        if (this.particleInterval) {
            clearInterval(this.particleInterval);
            this.particleInterval = null;
        }
    }
    
    calculateProductionPerSecond() {
        const pps = {};
        
        Object.keys(GameConfig.resources).forEach(resource => {
            pps[resource] = 0;
        });
        
        this.machines.forEach(machine => {
            if (machine.status === 'active' && machine.config.produces) {
                Object.keys(machine.config.produces).forEach(resource => {
                    const amount = machine.config.produces[resource];
                    pps[resource] = (pps[resource] || 0) + amount;
                });
            }
        });
        
        this.productionPerSecond = pps;
        return pps;
    }
    
    getTotalProduction() {
        return { ...this.totalProduction };
    }
    
    getProductionPerSecond() {
        return { ...this.productionPerSecond };
    }
    
    update(deltaTime) {
        this.lastUpdateTime += deltaTime;
        
        this.machines.forEach(machine => {
            const result = machine.update(deltaTime);
            
            if (result.production) {
                console.log(`[生产事件] ${machine.id}:`, result.production);
            }
        });
        
        this.calculateProductionPerSecond();
        
        if (this.lastUpdateTime >= 5000) {
            this.lastUpdateTime = 0;
            const activeCount = this.getActiveMachines().length;
            console.log(`[状态检查] 运行机械: ${activeCount}, 每秒产出:`, this.productionPerSecond);
        }
    }
}

if (typeof module !== 'undefined' && module.exports) {
    module.exports = { Machine, MachineManager };
}
