#!/usr/bin/env bash
# ============================================================
# 🚀 SHIPFOOD AI — SESSION INIT (IRON LAW 0)
# ============================================================
# Chạy khi bắt đầu session mới hoặc khi compliance check fail.
# Tự động: clean markers → check docs → scan skills → set markers
# ============================================================
# Usage:
#   bash scripts/session-init.sh          # Interactive mode
#   bash scripts/session-init.sh --force  # Auto mode (skip confirmations)
# ============================================================

MARKER_DIR="/tmp"
PROJECT_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BOLD='\033[1m'
NC='\033[0m'

FORCE=false
[[ "$1" == "--force" ]] && FORCE=true

echo ""
echo -e "${BOLD}============================================${NC}"
echo -e "${BOLD}  🚀 SHIPFOOD AI — SESSION INIT${NC}"
echo -e "${BOLD}============================================${NC}"
echo ""

# ── Step 1: Clean old markers ──────────────────────────────
echo -e "${BOLD}[1/10]${NC} Cleaning old compliance markers..."
rm -f "${MARKER_DIR}/.claude_read_today" \
      "${MARKER_DIR}/.project_md_read" \
      "${MARKER_DIR}/.uiux_md_read" \
      "${MARKER_DIR}/.skill_loaded" \
      "${MARKER_DIR}/.compliance_passed"
echo -e "       ${GREEN}✅ Markers cleaned${NC}"
echo ""

# ── Step 2: Check CLAUDE.md ────────────────────────────────
echo -e "${BOLD}[2/10]${NC} Checking CLAUDE.md..."
if [ -f "${PROJECT_ROOT}/CLAUDE.md" ]; then
    LINES=$(wc -l < "${PROJECT_ROOT}/CLAUDE.md")
    echo -e "       ${GREEN}✅ CLAUDE.md exists (${LINES} lines)${NC}"
    if [ "$FORCE" = false ]; then
        echo -e "       ${YELLOW}👉 Đọc CLAUDE.md rồi nhấn Enter để tiếp tục...${NC}"
        read -r
    fi
    touch "${MARKER_DIR}/.claude_read_today"
    echo -e "       Marker set: ${MARKER_DIR}/.claude_read_today"
else
    echo -e "       ${RED}❌ CLAUDE.md not found!${NC}"
    exit 1
fi
echo ""

# ── Step 3: Check Project.md ───────────────────────────────
echo -e "${BOLD}[3/10]${NC} Checking Project.md..."
if [ -f "${PROJECT_ROOT}/Project.md" ]; then
    LINES=$(wc -l < "${PROJECT_ROOT}/Project.md")
    echo -e "       ${GREEN}✅ Project.md exists (${LINES} lines)${NC}"
    if [ "$FORCE" = false ]; then
        echo -e "       ${YELLOW}👉 Đọc Project.md rồi nhấn Enter...${NC}"
        read -r
    fi
    touch "${MARKER_DIR}/.project_md_read"
    echo -e "       Marker set: ${MARKER_DIR}/.project_md_read"
else
    echo -e "       ${YELLOW}⚠️  Project.md not found — skipping${NC}"
fi
echo ""

# ── Step 4: Check UI-UX.md ─────────────────────────────────
echo -e "${BOLD}[4/10]${NC} Checking UI-UX.md..."
if [ -f "${PROJECT_ROOT}/UI-UX.md" ]; then
    LINES=$(wc -l < "${PROJECT_ROOT}/UI-UX.md")
    echo -e "       ${GREEN}✅ UI-UX.md exists (${LINES} lines)${NC}"
    if [ "$FORCE" = false ]; then
        echo -e "       ${YELLOW}👉 Đọc UI-UX.md rồi nhấn Enter...${NC}"
        read -r
    fi
    touch "${MARKER_DIR}/.uiux_md_read"
    echo -e "       Marker set: ${MARKER_DIR}/.uiux_md_read"
else
    echo -e "       ${YELLOW}⚠️  UI-UX.md not found — skipping${NC}"
fi
echo ""

# ── Step 5: Check fastship-rules.md ────────────────────────
echo -e "${BOLD}[5/10]${NC} Checking fastship-rules.md..."
if [ -f "${PROJECT_ROOT}/.agents/skills/fastship-rules.md" ]; then
    echo -e "       ${GREEN}✅ fastship-rules.md exists${NC}"
    if [ "$FORCE" = false ]; then
        echo -e "       ${YELLOW}👉 Đọc fastship-rules.md rồi nhấn Enter...${NC}"
        read -r
    fi
else
    echo -e "       ${YELLOW}⚠️  fastship-rules.md not found — skipping${NC}"
fi
echo ""

# ── Step 6: Scan all skills ────────────────────────────────
echo -e "${BOLD}[6/10]${NC} Scanning all skills (.agents/skills/)..."
SKILL_COUNT=0
if [ -d "${PROJECT_ROOT}/.agents/skills" ]; then
    for SKILL_DIR in "${PROJECT_ROOT}/.agents/skills/"*/; do
        SKILL_NAME=$(basename "$SKILL_DIR")
        SKILL_COUNT=$((SKILL_COUNT + 1))
    done
    echo -e "       ${GREEN}✅ Found ${SKILL_COUNT} skills${NC}"
else
    echo -e "       ${YELLOW}⚠️  .agents/skills/ not found${NC}"
fi
echo ""

# ── Step 7: Scan all repos (ShipFoodCore/Skills/) ──────────
echo -e "${BOLD}[7/10]${NC} Scanning all repos (ShipFoodCore/Skills/)..."
REPO_COUNT=0
if [ -d "${PROJECT_ROOT}/ShipFoodCore/Skills" ]; then
    for REPO_DIR in "${PROJECT_ROOT}/ShipFoodCore/Skills/"*/; do
        REPO_NAME=$(basename "$REPO_DIR")
        REPO_COUNT=$((REPO_COUNT + 1))
        echo -e "       📦 ${REPO_NAME}"
    done
    echo -e "       ${GREEN}✅ Found ${REPO_COUNT} repos${NC}"
else
    echo -e "       ${YELLOW}⚠️  ShipFoodCore/Skills/ not found${NC}"
fi
echo ""

# ── Step 8: Scan developer-icons ───────────────────────────
echo -e "${BOLD}[8/10]${NC} Scanning developer icons..."
ICON_COUNT=0
if [ -d "${PROJECT_ROOT}/ShipFoodCore/Skills/developer-icons-main/icons" ]; then
    ICON_COUNT=$(ls "${PROJECT_ROOT}/ShipFoodCore/Skills/developer-icons-main/icons/"*.svg 2>/dev/null | wc -l)
    echo -e "       ${GREEN}✅ Found ${ICON_COUNT} SVG icons${NC}"
fi
echo ""

# ── Step 9: Verify compliance-check.sh exists ──────────────
echo -e "${BOLD}[9/10]${NC} Verifying compliance-check.sh..."
if [ -f "${PROJECT_ROOT}/compliance-check.sh" ]; then
    echo -e "       ${GREEN}✅ compliance-check.sh ready${NC}"
else
    echo -e "       ${RED}❌ compliance-check.sh missing!${NC}"
    exit 1
fi
echo ""

# ── Step 10: Mark skill_loaded + run compliance ────────────
echo -e "${BOLD}[10/10]${NC} Finalizing..."
touch "${MARKER_DIR}/.skill_loaded"
echo -e "       Marker set: ${MARKER_DIR}/.skill_loaded"

echo ""
echo -e "${GREEN}${BOLD}  ✅ SESSION INIT COMPLETE — Running compliance check...${NC}"
echo ""

# Run compliance check
bash "${PROJECT_ROOT}/compliance-check.sh"
RESULT=$?

if [ $RESULT -eq 0 ]; then
    echo ""
    echo -e "${GREEN}${BOLD}  🚀 Ready to work!${NC}"
    echo ""
    exit 0
else
    echo ""
    echo -e "${RED}${BOLD}  ❌ Compliance check FAILED (exit $RESULT)${NC}"
    echo -e "  ${YELLOW}Check errors above and re-run:${NC}"
    echo "    rm -f /tmp/.claude_read_today /tmp/.skill_loaded"
    echo "    bash scripts/session-init.sh"
    echo ""
    exit 1
fi
