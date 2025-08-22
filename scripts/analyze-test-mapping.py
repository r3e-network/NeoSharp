#!/usr/bin/env python3
"""
Neo Swift to Neo Sharp SDK Test Mapping Analysis
This script analyzes the mapping between Swift and C# tests to ensure complete conversion coverage.
"""

import os
import re
import json
import sys
from pathlib import Path
from typing import Dict, List, Tuple, Set
from dataclasses import dataclass, asdict
from collections import defaultdict

@dataclass
class TestClass:
    name: str
    file_path: str
    test_methods: List[str]
    imports: List[str]
    framework: str  # 'swift' or 'csharp'

@dataclass
class ConversionMapping:
    swift_file: str
    csharp_file: str
    swift_tests: List[str]
    csharp_tests: List[str]
    missing_tests: List[str]
    extra_tests: List[str]
    conversion_rate: float

class TestMappingAnalyzer:
    def __init__(self, base_path: str):
        self.base_path = Path(base_path)
        self.swift_tests: Dict[str, TestClass] = {}
        self.csharp_tests: Dict[str, TestClass] = {}
        self.mappings: List[ConversionMapping] = []
        
    def analyze_swift_test_file(self, file_path: Path) -> TestClass:
        """Analyze a Swift test file to extract test methods and imports."""
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Extract class name
        class_match = re.search(r'class\s+(\w+Tests?)\s*:', content)
        class_name = class_match.group(1) if class_match else file_path.stem
        
        # Extract test methods (functions starting with 'test')
        test_methods = re.findall(r'func\s+(test\w+)\s*\(', content)
        
        # Extract imports
        imports = re.findall(r'import\s+(\w+)', content)
        
        return TestClass(
            name=class_name,
            file_path=str(file_path),
            test_methods=test_methods,
            imports=imports,
            framework='swift'
        )
    
    def analyze_csharp_test_file(self, file_path: Path) -> TestClass:
        """Analyze a C# test file to extract test methods and imports."""
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Extract class name
        class_match = re.search(r'public\s+class\s+(\w+Tests?)', content)
        class_name = class_match.group(1) if class_match else file_path.stem
        
        # Extract test methods (methods with [Fact] or [Theory] attributes)
        test_methods = []
        
        # Find [Fact] methods
        fact_matches = re.finditer(r'\[Fact\]\s*public\s+(?:async\s+Task|void)\s+(\w+)\s*\(', content)
        test_methods.extend([match.group(1) for match in fact_matches])
        
        # Find [Theory] methods
        theory_matches = re.finditer(r'\[Theory\].*?public\s+(?:async\s+Task|void)\s+(\w+)\s*\(', content, re.DOTALL)
        test_methods.extend([match.group(1) for match in theory_matches])
        
        # Extract using statements
        imports = re.findall(r'using\s+([^;]+);', content)
        imports = [imp.strip() for imp in imports]
        
        return TestClass(
            name=class_name,
            file_path=str(file_path),
            test_methods=test_methods,
            imports=imports,
            framework='csharp'
        )
    
    def find_swift_tests(self, swift_path: Path = None) -> None:
        """Find and analyze all Swift test files."""
        if swift_path is None:
            # Try to find Swift tests in common locations
            possible_paths = [
                self.base_path / "../NeoSwift-Reference/Tests",
                self.base_path / "../NeoSwift/Tests",
                self.base_path / "reference/swift-tests"
            ]
            swift_path = next((p for p in possible_paths if p.exists()), None)
        
        if swift_path is None or not swift_path.exists():
            print(f"⚠️  Warning: Swift tests directory not found. Checked: {[str(p) for p in possible_paths]}")
            return
        
        print(f"📁 Scanning Swift tests in: {swift_path}")
        
        for swift_file in swift_path.rglob("*Tests.swift"):
            try:
                test_class = self.analyze_swift_test_file(swift_file)
                self.swift_tests[test_class.name] = test_class
                print(f"   ✅ Found Swift test: {test_class.name} ({len(test_class.test_methods)} methods)")
            except Exception as e:
                print(f"   ❌ Error analyzing {swift_file}: {e}")
    
    def find_csharp_tests(self) -> None:
        """Find and analyze all C# test files."""
        csharp_path = self.base_path / "tests/NeoSharp.Tests"
        
        if not csharp_path.exists():
            print(f"❌ C# tests directory not found: {csharp_path}")
            return
        
        print(f"📁 Scanning C# tests in: {csharp_path}")
        
        for csharp_file in csharp_path.rglob("*Tests.cs"):
            try:
                test_class = self.analyze_csharp_test_file(csharp_file)
                self.csharp_tests[test_class.name] = test_class
                print(f"   ✅ Found C# test: {test_class.name} ({len(test_class.test_methods)} methods)")
            except Exception as e:
                print(f"   ❌ Error analyzing {csharp_file}: {e}")
    
    def create_mappings(self) -> None:
        """Create mappings between Swift and C# tests."""
        print("\n📊 Creating test mappings...")
        
        # Track which C# tests have been mapped
        mapped_csharp = set()
        
        for swift_name, swift_test in self.swift_tests.items():
            # Try to find corresponding C# test
            csharp_test = None
            
            # Direct name match
            if swift_name in self.csharp_tests:
                csharp_test = self.csharp_tests[swift_name]
            else:
                # Try fuzzy matching (remove suffix variations, case differences)
                for csharp_name, csharp_candidate in self.csharp_tests.items():
                    if self._names_match(swift_name, csharp_name):
                        csharp_test = csharp_candidate
                        break
            
            if csharp_test:
                mapped_csharp.add(csharp_test.name)
                mapping = self._create_mapping(swift_test, csharp_test)
            else:
                mapping = self._create_mapping(swift_test, None)
            
            self.mappings.append(mapping)
        
        # Add unmapped C# tests
        for csharp_name, csharp_test in self.csharp_tests.items():
            if csharp_name not in mapped_csharp:
                mapping = self._create_mapping(None, csharp_test)
                self.mappings.append(mapping)
    
    def _names_match(self, swift_name: str, csharp_name: str) -> bool:
        """Check if Swift and C# test names match (with variations)."""
        # Normalize names
        swift_normalized = swift_name.lower().replace("tests", "").replace("test", "")
        csharp_normalized = csharp_name.lower().replace("tests", "").replace("test", "")
        
        return swift_normalized == csharp_normalized
    
    def _create_mapping(self, swift_test: TestClass = None, csharp_test: TestClass = None) -> ConversionMapping:
        """Create a mapping between Swift and C# tests."""
        swift_file = swift_test.file_path if swift_test else "Missing"
        csharp_file = csharp_test.file_path if csharp_test else "Missing"
        swift_tests = swift_test.test_methods if swift_test else []
        csharp_tests = csharp_test.test_methods if csharp_test else []
        
        # Find missing and extra tests
        swift_set = set(swift_tests)
        csharp_set = set(csharp_tests)
        
        missing_tests = list(swift_set - csharp_set)
        extra_tests = list(csharp_set - swift_set)
        
        # Calculate conversion rate
        if swift_tests:
            conversion_rate = len(csharp_set & swift_set) / len(swift_set) * 100
        elif csharp_tests:
            conversion_rate = 100.0  # Extra C# tests (no Swift equivalent)
        else:
            conversion_rate = 0.0
        
        return ConversionMapping(
            swift_file=swift_file,
            csharp_file=csharp_file,
            swift_tests=swift_tests,
            csharp_tests=csharp_tests,
            missing_tests=missing_tests,
            extra_tests=extra_tests,
            conversion_rate=conversion_rate
        )
    
    def generate_report(self) -> Dict:
        """Generate a comprehensive analysis report."""
        report = {
            "summary": {
                "total_swift_tests": len(self.swift_tests),
                "total_csharp_tests": len(self.csharp_tests),
                "total_mappings": len(self.mappings),
                "fully_converted": 0,
                "partially_converted": 0,
                "missing_conversions": 0,
                "extra_csharp": 0,
                "overall_conversion_rate": 0.0
            },
            "detailed_mappings": [],
            "missing_files": [],
            "conversion_issues": []
        }
        
        total_swift_methods = 0
        total_converted_methods = 0
        
        for mapping in self.mappings:
            # Update summary statistics
            if mapping.swift_file == "Missing":
                report["summary"]["extra_csharp"] += 1
            elif mapping.csharp_file == "Missing":
                report["summary"]["missing_conversions"] += 1
            elif mapping.conversion_rate == 100.0:
                report["summary"]["fully_converted"] += 1
            else:
                report["summary"]["partially_converted"] += 1
            
            # Track method conversion
            if mapping.swift_tests:
                total_swift_methods += len(mapping.swift_tests)
                converted_count = len(set(mapping.swift_tests) & set(mapping.csharp_tests))
                total_converted_methods += converted_count
            
            # Add to detailed mappings
            mapping_dict = asdict(mapping)
            report["detailed_mappings"].append(mapping_dict)
            
            # Track missing files
            if mapping.csharp_file == "Missing":
                report["missing_files"].append({
                    "swift_file": mapping.swift_file,
                    "expected_csharp_file": mapping.swift_file.replace(".swift", ".cs"),
                    "test_count": len(mapping.swift_tests)
                })
            
            # Track conversion issues
            if mapping.missing_tests:
                report["conversion_issues"].append({
                    "file": mapping.csharp_file,
                    "type": "missing_tests",
                    "details": mapping.missing_tests
                })
        
        # Calculate overall conversion rate
        if total_swift_methods > 0:
            report["summary"]["overall_conversion_rate"] = (total_converted_methods / total_swift_methods) * 100
        
        return report
    
    def print_summary(self, report: Dict) -> None:
        """Print a human-readable summary of the analysis."""
        summary = report["summary"]
        
        print("\n" + "="*60)
        print("📈 CONVERSION ANALYSIS SUMMARY")
        print("="*60)
        
        print(f"📊 Test Files:")
        print(f"   • Swift test classes: {summary['total_swift_tests']}")
        print(f"   • C# test classes: {summary['total_csharp_tests']}")
        print(f"   • Total mappings analyzed: {summary['total_mappings']}")
        
        print(f"\n🎯 Conversion Status:")
        print(f"   • Fully converted: {summary['fully_converted']} ✅")
        print(f"   • Partially converted: {summary['partially_converted']} ⚠️")
        print(f"   • Missing conversions: {summary['missing_conversions']} ❌")
        print(f"   • Extra C# tests: {summary['extra_csharp']} 🆕")
        
        print(f"\n📈 Overall Conversion Rate: {summary['overall_conversion_rate']:.1f}%")
        
        # Show most problematic conversions
        if report["conversion_issues"]:
            print(f"\n⚠️  Top Conversion Issues:")
            for issue in report["conversion_issues"][:5]:  # Show top 5
                print(f"   • {Path(issue['file']).name}: {len(issue['details'])} missing tests")
        
        # Show missing files
        if report["missing_files"]:
            print(f"\n❌ Missing C# Test Files:")
            for missing in report["missing_files"][:5]:  # Show top 5
                swift_name = Path(missing['swift_file']).name
                print(f"   • {swift_name} → {missing['expected_csharp_file']} ({missing['test_count']} tests)")
    
    def save_report(self, report: Dict, filename: str = "test-mapping-analysis.json") -> None:
        """Save the detailed report to a JSON file."""
        output_path = self.base_path / filename
        with open(output_path, 'w', encoding='utf-8') as f:
            json.dump(report, f, indent=2, ensure_ascii=False)
        
        print(f"\n💾 Detailed report saved to: {output_path}")
    
    def run_analysis(self, swift_path: str = None) -> Dict:
        """Run the complete test mapping analysis."""
        print("🚀 Starting Neo Swift to Neo Sharp SDK Test Mapping Analysis")
        print(f"📂 Base directory: {self.base_path}")
        
        # Find and analyze tests
        self.find_swift_tests(Path(swift_path) if swift_path else None)
        self.find_csharp_tests()
        
        if not self.swift_tests and not self.csharp_tests:
            print("❌ No test files found! Please check your directory paths.")
            return {}
        
        # Create mappings
        self.create_mappings()
        
        # Generate and display report
        report = self.generate_report()
        self.print_summary(report)
        self.save_report(report)
        
        return report

def main():
    import argparse
    
    parser = argparse.ArgumentParser(description="Analyze Neo Swift to Neo Sharp SDK test conversion mapping")
    parser.add_argument("--base-path", default=".", help="Base path to the Neo Sharp SDK project")
    parser.add_argument("--swift-path", help="Path to Swift test files (if different from auto-detection)")
    parser.add_argument("--output", default="test-mapping-analysis.json", help="Output JSON report filename")
    
    args = parser.parse_args()
    
    analyzer = TestMappingAnalyzer(args.base_path)
    report = analyzer.run_analysis(args.swift_path)
    
    if args.output != "test-mapping-analysis.json":
        analyzer.save_report(report, args.output)
    
    # Return appropriate exit code
    if report and report["summary"]["overall_conversion_rate"] >= 90:
        print("\n🎉 Conversion analysis completed successfully!")
        sys.exit(0)
    else:
        print("\n⚠️  Conversion analysis completed with issues to address.")
        sys.exit(1)

if __name__ == "__main__":
    main()