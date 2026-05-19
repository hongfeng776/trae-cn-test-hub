const GameConfig = {
    game: {
        name: '蒸汽机械工坊',
        version: '1.2.0',
        tickRate: 60,
    },
    
    production: {
        interval: 1000,
    },
    
    resources: {
        coal: {
            name: '煤炭',
            initialAmount: 50,
            maxAmount: 200,
            perClick: 10,
            icon: '🔥',
        },
        steam: {
            name: '蒸汽',
            initialAmount: 0,
            maxAmount: 100,
            pressurePerMachine: 10,
            icon: '💨',
        },
        gears: {
            name: '齿轮零件',
            initialAmount: 50,
            maxAmount: 1000,
            perSecond: 2,
            icon: '⚙️',
        },
        plates: {
            name: '金属板',
            initialAmount: 30,
            maxAmount: 500,
            perSecond: 3,
            icon: '🔩',
        },
        pipes: {
            name: '蒸汽管道',
            initialAmount: 20,
            maxAmount: 300,
            perSecond: 1,
            icon: '🛠️',
        },
        'advanced-gears': {
            name: '高阶齿轮组',
            initialAmount: 0,
            maxAmount: 100,
            perSecond: 0,
            icon: '⚙️',
        },
        'reinforced-plates': {
            name: '强化金属板',
            initialAmount: 0,
            maxAmount: 50,
            perSecond: 0,
            icon: '🔩',
        },
        'steam-engine-core': {
            name: '蒸汽机核心',
            initialAmount: 0,
            maxAmount: 10,
            perSecond: 0,
            icon: '💎',
        },
    },
    
    recipes: [
        {
            id: 'advanced-gears',
            name: '高阶齿轮组',
            icon: '⚙️',
            description: '精密组装的高阶齿轮传动系统',
            ingredients: {
                gears: 10,
                plates: 5,
            },
            output: {
                'advanced-gears': 1,
            },
            craftTime: 3000,
        },
        {
            id: 'reinforced-plates',
            name: '强化金属板',
            icon: '🔩',
            description: '高压锻造的强化金属板材',
            ingredients: {
                plates: 8,
                pipes: 3,
            },
            output: {
                'reinforced-plates': 1,
            },
            craftTime: 4000,
        },
        {
            id: 'steam-engine-core',
            name: '蒸汽机核心',
            icon: '💎',
            description: '精密蒸汽动力核心组件',
            ingredients: {
                'advanced-gears': 2,
                'reinforced-plates': 1,
                pipes: 5,
            },
            output: {
                'steam-engine-core': 1,
            },
            craftTime: 8000,
        },
    ],
    
    machines: {
        'steam-engine': {
            name: '蒸汽机',
            baseProduction: 5,
            steamConsumption: 2,
            coalConsumption: 1,
            produces: {
                gears: 2,
                pipes: 1,
            },
            idleAnimation: {
                enabled: true,
                duration: 2000,
                type: 'bob',
            },
            activeAnimation: {
                pistonSpeed: 500,
                flywheelSpeed: 1000,
                gearSpeed: 2000,
            },
        },
        'press-machine': {
            name: '冲压机',
            baseProduction: 8,
            steamConsumption: 3,
            coalConsumption: 0,
            produces: {
                plates: 3,
                gears: 1,
            },
            idleAnimation: {
                enabled: true,
                duration: 2500,
                type: 'bob',
            },
            activeAnimation: {
                pressSpeed: 1500,
                leverSpeed: 1500,
            },
        },
        'coal-furnace': {
            name: '燃煤锅炉',
            baseProduction: 15,
            steamConsumption: 0,
            coalConsumption: 2,
            produces: {
                steam: 15,
            },
            idleAnimation: {
                enabled: false,
            },
            activeAnimation: {
                fireSpeed: 500,
                smokeInterval: 1000,
            },
        },
    },
    
    particles: {
        steam: {
            enabled: true,
            spawnInterval: 500,
            maxParticles: 20,
            riseDuration: 3000,
            startOpacity: 0.6,
            endOpacity: 0,
            startScale: 1,
            endScale: 2,
        },
    },
    
    ui: {
        updateInterval: 100,
        statusColors: {
            idle: '#a0a0a0',
            active: '#00ff00',
            stopped: '#ff0000',
        },
    },
    
    sounds: {
        enabled: false,
        machineStart: '',
        machineStop: '',
        steamRelease: '',
        coalAdd: '',
    },
    
    initialMachines: [
        { id: 'steam-engine-1', type: 'steam-engine' },
        { id: 'steam-engine-2', type: 'steam-engine' },
        { id: 'press-machine-1', type: 'press-machine' },
        { id: 'coal-furnace', type: 'coal-furnace' },
    ],
};

if (typeof module !== 'undefined' && module.exports) {
    module.exports = GameConfig;
}
