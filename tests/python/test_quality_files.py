"""Quality and community metadata tests."""

from __future__ import annotations

import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def test_translation_and_icon_files_are_valid_json() -> None:
    json.loads((ROOT / "custom_components/gwm_ora/translations/en.json").read_text(encoding="utf-8"))
    json.loads((ROOT / "custom_components/gwm_ora/icons.json").read_text(encoding="utf-8"))


def test_community_health_files_exist() -> None:
    for path in (
        "CODE_OF_CONDUCT.md",
        "CONTRIBUTING.md",
        "LICENSE",
        "README.md",
        "SECURITY.md",
        "SUPPORT.md",
        ".github/pull_request_template.md",
        ".github/ISSUE_TEMPLATE/bug_report.yml",
        ".github/ISSUE_TEMPLATE/feature_request.yml",
        ".github/ISSUE_TEMPLATE/config.yml",
    ):
        assert (ROOT / path).is_file()


def test_quality_scale_tracker_exists() -> None:
    quality_scale = ROOT / "custom_components/gwm_ora/quality_scale.yaml"
    text = quality_scale.read_text(encoding="utf-8")

    assert "config-flow: done" in text
    assert "diagnostics: done" in text
    assert "reconfiguration-flow: done" in text
    assert "test-coverage:" in text
