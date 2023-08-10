class TimeUtility {
    public static formatRelativeTimestamp(timestamp: Date) {
        const now = new Date();
        // @ts-ignore
        const diff = now - timestamp;
        const suffix = diff < 0 ? 'from now' : 'ago';

        const seconds = Math.floor(diff / 1000);
        if (seconds < 60) {
            return `${seconds} second${seconds !== 1 ? 's' : ''} ${suffix}`;
        }

        const minutes = Math.floor(diff / 60000);
        if (minutes < 60) {
            return `${minutes} minute${minutes !== 1 ? 's' : ''} ${suffix}`;
        }

        const hours = Math.floor(diff / 3600000);
        if (hours < 24) {
            return `${hours} hour${hours !== 1 ? 's' : ''} ${suffix}`;
        }

        const days = Math.floor(diff / 86400000);
        if (days < 30) {
            return `${days} day${days !== 1 ? 's' : ''} ${suffix}`;
        }

        const months = Math.floor(diff / 2592000000);
        if (months < 12) {
            return `${months} month${months !== 1 ? 's' : ''} ${suffix}`;
        }

        const years = Math.floor(diff / 31536000000);
        return `${years} year${years !== 1 ? 's' : ''} ${suffix}`;
    };
}

export default TimeUtility;