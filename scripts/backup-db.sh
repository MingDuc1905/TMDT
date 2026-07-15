#!/bin/bash
# ═══ FastShip DB Backup Script (P0 — Backup Strategy) ═══
# Ch?y: bash scripts/backup-db.sh
# Config qua env vars:
#   DATABASE_URL — Render PostgreSQL URL (t? d?ng parse)
#   BACKUP_DIR — n?i l?u file backup (m?c d?nh: ./backups/)
#   BACKUP_RETENTION_DAYS — gi? l?i bao nhiêu ngày (m?c d?nh: 7)
#
# Nên c?u vào cron: 0 3 * * * bash /app/scripts/backup-db.sh

set -euo pipefail

BACKUP_DIR="${BACKUP_DIR:-./backups}"
RETENTION_DAYS="${BACKUP_RETENTION_DAYS:-7}"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
DB_URL="${DATABASE_URL:-}"

mkdir -p "$BACKUP_DIR"

if [ -z "$DB_URL" ]; then
    echo "❌ DATABASE_URL not set. Cannot backup."
    exit 1
fi

BACKUP_FILE="$BACKUP_DIR/fastship_$TIMESTAMP.sql.gz"
LOG_FILE="$BACKUP_DIR/backup.log"

echo "[$(date)] Starting backup..." | tee -a "$LOG_FILE"

# Backup using pg_dump via DATABASE_URL
pg_dump "$DB_URL" --no-owner --no-acl | gzip > "$BACKUP_FILE"

# Verify backup
if [ -f "$BACKUP_FILE" ] && [ -s "$BACKUP_FILE" ]; then
    SIZE=$(du -h "$BACKUP_FILE" | cut -f1)
    echo "[$(date)] ✅ Backup created: $BACKUP_FILE ($SIZE)" | tee -a "$LOG_FILE"
else
    echo "[$(date)] ❌ Backup failed!" | tee -a "$LOG_FILE"
    exit 1
fi

# Clean old backups
find "$BACKUP_DIR" -name "fastship_*.sql.gz" -mtime +$RETENTION_DAYS -delete
echo "[$(date)] ✅ Old backups cleaned (retention: ${RETENTION_DAYS}d)" | tee -a "$LOG_FILE"
echo "[$(date)] ✅ Backup complete" | tee -a "$LOG_FILE"
