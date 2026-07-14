#!/usr/bin/env bash
# ============================================================
# 📌 SHIPFOOD AI — MARK-READ (Set compliance markers)
# ============================================================
# Dùng để set /tmp/ markers sau khi đọc docs.
# ============================================================
# Usage:
#   bash scripts/mark-read.sh claude     # Đã đọc CLAUDE.md
#   bash scripts/mark-read.sh project    # Đã đọc Project.md
#   bash scripts/mark-read.sh uiux       # Đã đọc UI-UX.md
#   bash scripts/mark-read.sh skill      # Đã load skill
#   bash scripts/mark-read.sh all        # Set TẤT CẢ markers
#   bash scripts/mark-read.sh status     # Xem trạng thái
# ============================================================

MARKER_DIR="/tmp"
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BOLD='\033[1m'
NC='\033[0m'

ACTION="${1:-status}"

case "$ACTION" in
    claude)
        touch "${MARKER_DIR}/.claude_read_today"
        echo -e "${GREEN}✅${NC} Marker set: CLAUDE.md đã đọc"
        ;;
    project)
        touch "${MARKER_DIR}/.project_md_read"
        echo -e "${GREEN}✅${NC} Marker set: Project.md đã đọc"
        ;;
    uiux)
        touch "${MARKER_DIR}/.uiux_md_read"
        echo -e "${GREEN}✅${NC} Marker set: UI-UX.md đã đọc"
        ;;
    skill)
        touch "${MARKER_DIR}/.skill_loaded"
        echo -e "${GREEN}✅${NC} Marker set: Skill đã load"
        ;;
    all)
        touch "${MARKER_DIR}/.claude_read_today"
        touch "${MARKER_DIR}/.project_md_read"
        touch "${MARKER_DIR}/.uiux_md_read"
        touch "${MARKER_DIR}/.skill_loaded"
        echo -e "${GREEN}✅${NC} All markers set"
        ;;
    clean)
        rm -f "${MARKER_DIR}/.claude_read_today" \
              "${MARKER_DIR}/.project_md_read" \
              "${MARKER_DIR}/.uiux_md_read" \
              "${MARKER_DIR}/.skill_loaded" \
              "${MARKER_DIR}/.compliance_passed"
        echo -e "${YELLOW}🧹${NC} All markers cleaned"
        ;;
    status)
        echo -e "${BOLD}📊 Compliance marker status:${NC}"
        for M in claude_read_today project_md_read uiux_md_read skill_loaded compliance_passed; do
            if [ -f "${MARKER_DIR}/.${M}" ]; then
                MTIME=$(date -r "${MARKER_DIR}/.${M}" '+%H:%M' 2>/dev/null || echo "?")
                echo -e "  ${GREEN}✅${NC} .${M}  (${MTIME})"
            else
                echo -e "  ${RED}❌${NC} .${M}"
            fi
        done
        ;;
    *)
        echo -e "${YELLOW}Usage:${NC} bash scripts/mark-read.sh {claude|project|uiux|skill|all|clean|status}"
        exit 1
        ;;
esac
