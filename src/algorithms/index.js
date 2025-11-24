import trialDivision from './trialDivision.js'
import trialDivisionOptimized from './trialDivisionOptimized.js'
import eratosthenesSieve from './eratosthenesSieve.js'
import segmentedSieve from './segmentedSieve.js'

export const algorithmsList = {
    trial: {
        func: trialDivision,
        label: 'Перебір (наївний)',
        color: '#dc3545'
    },

    trialOpt: {
        func: trialDivisionOptimized,
        label: 'Перебір (оптимізований)',
        color: '#dc3545'
    },

    eratosthenes: {
        func: eratosthenesSieve,
        label: 'Решето Ератосфена',
        color: '#dc3545'
    },

    segmented: {
        func: segmentedSieve,
        label: 'Сегментоване решето',
        color: '#dc3545'
    }
}

export default function getAlgorithm(key){
    return algorithmsList[key]?.func
}