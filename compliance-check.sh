#!/usr/bin/env bash
# ============================================================
# 🚨 COMPLIANCE PRE-FLIGHT CHECK — ShipFood AI
# Chạy mỗi đầu response (sau skill load) để verify compliance
# ============================================================
# Exit codes:
#   0 ✅ All checks passed
#   1 ❌ CLAUDE.md chưa đọc
#   2 ❌ Project.md chưa đọc
#   3 ❌ UI-UX.md chưa đọc
#   4 ❌ Skills chưa load
#   5 ❌ Compliance chưa pass hoặc quá cũ (>30 phút)
# ============================================================

MARKER_DIR="/tmp"
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BOLD='\033[1m'
NC='\033[0m'

echo ""
echo -e "${BOLD}============================================${NC}"
echo -e "${BOLD}  🚨 COMPLIANCE PRE-FLIGHT CHECK${NC}"
echo -e "${BOLD}============================================${NC}"
echo ""

PASSED=true

# ---- Check 1: CLAUDE.md read ----
if [ -f "${MARKER_DIR}/.claude_read_today" ]; then
    echo -e "  [1/5] 📖 CLAUDE.md     ${GREEN}✅ Đã đọc${NC}"
else
    echo -e "  [1/5] 📖 CLAUDE.md     ${RED}❌ CHƯA ĐỌC${NC}"
    PASSED=false
fi

# ---- Check 2: Project.md read ----
if [ -f "${MARKER_DIR}/.project_md_read" ]; then
    echo -e "  [2/5] 📖 Project.md    ${GREEN}✅ Đã đọc${NC}"
else
    echo -e "  [2/5] 📖 Project.md    ${RED}❌ CHƯA ĐỌC${NC}"
    PASSED=false
fi

# ---- Check 3: UI-UX.md read ----
if [ -f "${MARKER_DIR}/.uiux_md_read" ]; then
    echo -e "  [3/5] 📖 UI-UX.md      ${GREEN}✅ Đã đọc${NC}"
else
    echo -e "  [3/5] 📖 UI-UX.md      ${RED}❌ CHƯA ĐỌC${NC}"
    PASSED=false
fi

# ---- Check 4: Skill loaded ----
if [ -f "${MARKER_DIR}/.skill_loaded" ]; then
    echo -e "  [4/5] 🎯 Skill loaded  ${GREEN}✅ Đã load${NC}"
else
    echo -e "  [4/5] 🎯 Skill loaded  ${RED}❌ CHƯA LOAD${NC}"
    PASSED=false
fi

# ---- Check 5: Compliance timestamp (< 30 phút) ----
CHECK5="${MARKER_DIR}/.compliance_passed"
MAX_AGE=1800
if [ -f "$CHECK5" ]; then
    # date -r works on Linux, macOS, and Windows git bash
    FILE_TS=$(date -r "$CHECK5" +%s 2>/dev/null || echo 0)
    NOW_TS=$(date +%s)
    FILE_AGE=$((NOW_TS - FILE_TS))
    if [ "$FILE_AGE" -lt "$MAX_AGE" ] 2>/dev/null; then
        FILE_TIME=$(date -r "$CHECK5" '+%H:%M' 2>/dev/null || echo "?")
        echo -e "  [5/5] ⏱️  Compliance   ${GREEN}✅ Passed $FILE_TIME${NC}"
    else
        echo -e "  [5/5] ⏱️  Compliance   ${YELLOW}⚠️  Quá cũ (>30 phút)${NC}"
        PASSED=false
    fi
else
    echo -e "  [5/5] ⏱️  Compliance   ${RED}❌ CHƯA PASS${NC}"
    PASSED=false
fi

echo ""

if [ "$PASSED" = true ]; then
    echo -e "${GREEN}${BOLD}  ✅ ALL CHECKS PASSED — Ready to work!${NC}"

    # Quick context stats
    SELF_DIR="$(cd "$(dirname "$0")" && pwd)"
    SKILL_COUNT=$(ls "${SELF_DIR}/.agents/skills/" 2>/dev/null | wc -l)
    REPO_COUNT=$(ls -d "${SELF_DIR}/ShipFoodCore/Skills/"*/ 2>/dev/null | wc -l)
    ICON_COUNT=$(ls "${SELF_DIR}/ShipFoodCore/Skills/developer-icons-main/icons/" 2>/dev/null | wc -l)

    echo ""
    echo -e "  📊 Context summary:"
    echo -e "     Skills: ${SKILL_COUNT:-?} | Repos: ${REPO_COUNT:-?} | SVG icons: ${ICON_COUNT:-?}"
    echo ""
    exit 0
else
    echo -e "${RED}${BOLD}  ❌ SOME CHECKS FAILED — Cannot proceed!${NC}"
    echo ""
    echo -e "  ${YELLOW}Run session init:${NC}"
    echo "    rm -f /tmp/.claude_read_today /tmp/.project_md_read /tmp/.uiux_md_read /tmp/.skill_loaded /tmp/.compliance_passed"
    echo "    # Then: read CLAUDE.md → Project.md → UI-UX.md → scan skills → load skill → mark completion"
    echo ""
    exit 5
fi
