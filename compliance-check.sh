#!/usr/bin/env bash
# ============================================================
# 🚨 COMPLIANCE PRE-FLIGHT CHECK — ShipFood AI v2.0
# Chạy mỗi đầu response (sau skill load) để verify compliance
# Bao gồm IRON LAW 7: Resource Utilization verification
# ============================================================
# Exit codes:
#   0 ✅ All checks passed
#   1 ❌ CLAUDE.md chưa đọc
#   2 ❌ Project.md chưa đọc
#   3 ❌ UI-UX.md chưa đọc
#   4 ❌ Skills chưa load
#   5 ❌ Compliance chưa pass hoặc quá cũ (>30 phút)
#   6 ❌ Resource scan chưa hoàn tất (IRON LAW 7)
# ============================================================

MARKER_DIR="/tmp"
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BOLD='\033[1m'
NC='\033[0m'

echo ""
echo -e "${BOLD}============================================${NC}"
echo -e "${BOLD}  🚨 COMPLIANCE PRE-FLIGHT CHECK v2.0${NC}"
echo -e "${BOLD}============================================${NC}"
echo ""

PASSED=true
SELF_DIR="$(cd "$(dirname "$0")" && pwd)"

# ---- Check 1: CLAUDE.md read ----
if [ -f "${MARKER_DIR}/.claude_read_today" ]; then
    echo -e "  [1/7] 📖 CLAUDE.md     ${GREEN}✅ Đã đọc${NC}"
else
    echo -e "  [1/7] 📖 CLAUDE.md     ${RED}❌ CHƯA ĐỌC${NC}"
    PASSED=false
fi

# ---- Check 2: Project.md read ----
if [ -f "${MARKER_DIR}/.project_md_read" ]; then
    echo -e "  [2/7] 📖 Project.md    ${GREEN}✅ Đã đọc${NC}"
else
    echo -e "  [2/7] 📖 Project.md    ${RED}❌ CHƯA ĐỌC${NC}"
    PASSED=false
fi

# ---- Check 3: UI-UX.md read ----
if [ -f "${MARKER_DIR}/.uiux_md_read" ]; then
    echo -e "  [3/7] 📖 UI-UX.md      ${GREEN}✅ Đã đọc${NC}"
else
    echo -e "  [3/7] 📖 UI-UX.md      ${RED}❌ CHƯA ĐỌC${NC}"
    PASSED=false
fi

# ---- Check 4: Skill loaded ----
if [ -f "${MARKER_DIR}/.skill_loaded" ]; then
    echo -e "  [4/7] 🎯 Skill loaded  ${GREEN}✅ Đã load${NC}"
else
    echo -e "  [4/7] 🎯 Skill loaded  ${RED}❌ CHƯA LOAD${NC}"
    PASSED=false
fi

# ---- Check 5: Compliance timestamp (< 30 phút) ----
CHECK5="${MARKER_DIR}/.compliance_passed"
MAX_AGE=1800
if [ -f "$CHECK5" ]; then
    FILE_TS=$(date -r "$CHECK5" +%s 2>/dev/null || echo 0)
    NOW_TS=$(date +%s)
    FILE_AGE=$((NOW_TS - FILE_TS))
    if [ "$FILE_AGE" -lt "$MAX_AGE" ] 2>/dev/null; then
        FILE_TIME=$(date -r "$CHECK5" '+%H:%M' 2>/dev/null || echo "?")
        echo -e "  [5/7] ⏱️  Compliance   ${GREEN}✅ Passed $FILE_TIME${NC}"
    else
        echo -e "  [5/7] ⏱️  Compliance   ${YELLOW}⚠️  Quá cũ (>30 phút)${NC}"
        PASSED=false
    fi
else
    echo -e "  [5/7] ⏱️  Compliance   ${RED}❌ CHƯA PASS${NC}"
    PASSED=false
fi

# ---- Check 6: Resource scan count (IRON LAW 7) ----
SKILL_COUNT=$(ls "${SELF_DIR}/.agents/skills/" 2>/dev/null | wc -l)
REPO_COUNT=$(ls -d "${SELF_DIR}/ShipFoodCore/Skills/"*/ 2>/dev/null | wc -l)
ICON_COUNT=$(ls "${SELF_DIR}/ShipFoodCore/Skills/developer-icons-main/icons/" 2>/dev/null | wc -l)

if [ "$SKILL_COUNT" -eq 188 ] 2>/dev/null; then
    echo -e "  [6/7] 📦 188 skills    ${GREEN}✅ Đủ ($SKILL_COUNT)${NC}"
else
    echo -e "  [6/7] 📦 188 skills    ${YELLOW}⚠️  Lệch: $SKILL_COUNT (mong đợi 188)${NC}"
fi

if [ "$REPO_COUNT" -eq 12 ] 2>/dev/null; then
    echo -e "  [6/7] 📚 12 repos      ${GREEN}✅ Đủ ($REPO_COUNT)${NC}"
else
    echo -e "  [6/7] 📚 12 repos      ${YELLOW}⚠️  Lệch: $REPO_COUNT (mong đợi 12)${NC}"
fi

if [ "$ICON_COUNT" -eq 320 ] 2>/dev/null; then
    echo -e "  [6/7] 🎨 320 icons    ${GREEN}✅ Đủ ($ICON_COUNT)${NC}"
elif [ "$ICON_COUNT" -gt 0 ] 2>/dev/null; then
    echo -e "  [6/7] 🎨 $ICON_COUNT icons    ${YELLOW}⚠️  Lệch: $ICON_COUNT (mong đợi 320)${NC}"
else
    echo -e "  [6/7] 🎨 Icons        ${YELLOW}⚠️  Không tìm thấy icons${NC}"
fi

# ---- Check 7: Resource scan marker ----
if [ -f "${MARKER_DIR}/.resource_scanned" ]; then
    echo -e "  [7/7] 🔍 IRON LAW 7   ${GREEN}✅ Resource scan done${NC}"
else
    echo -e "  [7/7] 🔍 IRON LAW 7   ${YELLOW}⚠️  Resource scan chưa xác nhận${NC}"
    echo -e "  ${YELLOW}  → Chạy: touch /tmp/.resource_scanned sau khi scan 188 skills + 12 repos${NC}"
fi

echo ""

if [ "$PASSED" = true ]; then
    echo -e "${GREEN}${BOLD}  ✅ ALL CHECKS PASSED — Ready to work!${NC}"
    echo ""
    echo -e "  📊 Context summary:"
    echo -e "     Skills: ${SKILL_COUNT:-?} / 188 | Repos: ${REPO_COUNT:-?} / 12 | SVG icons: ${ICON_COUNT:-?} / 320"
    echo -e "     Design patterns: awesome-claude-design | UI rules: ui-ux-pro-max-skill-main"
    echo -e "     Security suite: gstack-main | E2E: lightpanda-browser | APIs: public-apis-master"
    echo ""
    echo -e "  ${BOLD}🔴 IRON LAW 7 ACTIVE — Phải dùng triệt để tài nguyên!${NC}"
    echo ""
    exit 0
else
    echo -e "${RED}${BOLD}  ❌ SOME CHECKS FAILED — Cannot proceed!${NC}"
    echo ""
    echo -e "  ${YELLOW}Run session init:${NC}"
    echo "    rm -f /tmp/.claude_read_today /tmp/.project_md_read /tmp/.uiux_md_read /tmp/.skill_loaded /tmp/.compliance_passed"
    echo "    # Then: read CLAUDE.md → Project.md → UI-UX.md → scan 188 skills + 12 repos → load skill → mark completion"
    echo ""
    exit 5
fi
