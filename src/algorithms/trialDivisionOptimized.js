export default function trialDivisionOptimized(n){
    if(n == 1) return false
    if (n == 2) return true

    let i = 2
    let k = Math.ceil(Math.sqrt(n))

    while (i <= k){

        if(n % i == 0){
            return false
        }

        i++
    }

    return true
}