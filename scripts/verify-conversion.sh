#!/bin/bash

# Neo Swift to Neo Sharp SDK Conversion Verification Script
# This script verifies that all Swift SDK unit tests have been properly converted to C# Sharp SDK

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Counters
TOTAL_CHECKS=0
PASSED_CHECKS=0
FAILED_CHECKS=0

# Log files
VERIFICATION_LOG="verification-report.log"
ERROR_LOG="conversion-errors.log"

# Clear previous logs
> "$VERIFICATION_LOG"
> "$ERROR_LOG"

echo "=== Neo Swift to Neo Sharp SDK Conversion Verification ===" | tee -a "$VERIFICATION_LOG"
echo "Started: $(date)" | tee -a "$VERIFICATION_LOG"
echo "" | tee -a "$VERIFICATION_LOG"

# Function to log and display results
log_result() {
    local status=$1
    local message=$2
    local details=$3
    
    TOTAL_CHECKS=$((TOTAL_CHECKS + 1))
    
    if [ "$status" = "PASS" ]; then
        echo -e "${GREEN}✅ PASS${NC}: $message" | tee -a "$VERIFICATION_LOG"
        PASSED_CHECKS=$((PASSED_CHECKS + 1))
    elif [ "$status" = "FAIL" ]; then
        echo -e "${RED}❌ FAIL${NC}: $message" | tee -a "$VERIFICATION_LOG"
        [ -n "$details" ] && echo "   Details: $details" | tee -a "$VERIFICATION_LOG"
        echo "$message: $details" >> "$ERROR_LOG"
        FAILED_CHECKS=$((FAILED_CHECKS + 1))
    elif [ "$status" = "WARN" ]; then
        echo -e "${YELLOW}⚠️  WARN${NC}: $message" | tee -a "$VERIFICATION_LOG"
        [ -n "$details" ] && echo "   Details: $details" | tee -a "$VERIFICATION_LOG"
    else
        echo -e "${BLUE}ℹ️  INFO${NC}: $message" | tee -a "$VERIFICATION_LOG"
    fi
}

# Function to check if directory exists
check_directory() {
    local dir=$1
    local description=$2
    
    if [ -d "$dir" ]; then
        log_result "PASS" "Directory exists: $description" "$dir"
        return 0
    else
        log_result "FAIL" "Directory missing: $description" "$dir"
        return 1
    fi
}

# Function to count files
count_files() {
    local pattern=$1
    local dir=$2
    find "$dir" -name "$pattern" 2>/dev/null | wc -l
}

# Function to check build status
check_build() {
    local project_path=$1
    local project_name=$2
    
    echo "" | tee -a "$VERIFICATION_LOG"
    echo "=== BUILD VERIFICATION: $project_name ===" | tee -a "$VERIFICATION_LOG"
    
    cd "$project_path"
    
    # Clean build
    if dotnet clean > /dev/null 2>&1; then
        log_result "PASS" "Project clean successful" "$project_name"
    else
        log_result "FAIL" "Project clean failed" "$project_name"
        return 1
    fi
    
    # Build project
    local build_output
    build_output=$(dotnet build 2>&1)
    local build_result=$?
    
    if [ $build_result -eq 0 ]; then
        log_result "PASS" "Project builds successfully" "$project_name"
        
        # Check for warnings
        local warning_count
        warning_count=$(echo "$build_output" | grep -c "warning" || true)
        if [ "$warning_count" -gt 0 ]; then
            log_result "WARN" "Build has $warning_count warnings" "$project_name"
        fi
        
        return 0
    else
        local error_count
        error_count=$(echo "$build_output" | grep -c "error" || true)
        log_result "FAIL" "Project build failed with $error_count errors" "$project_name"
        
        # Log first 10 errors for analysis
        echo "$build_output" | grep "error" | head -10 >> "$ERROR_LOG"
        return 1
    fi
}

# Function to run tests
run_tests() {
    local project_path=$1
    local project_name=$2
    
    echo "" | tee -a "$VERIFICATION_LOG"
    echo "=== TEST EXECUTION: $project_name ===" | tee -a "$VERIFICATION_LOG"
    
    cd "$project_path"
    
    # Run tests
    local test_output
    test_output=$(dotnet test --verbosity minimal 2>&1)
    local test_result=$?
    
    if [ $test_result -eq 0 ]; then
        # Extract test statistics
        local total_tests
        local passed_tests
        local failed_tests
        local skipped_tests
        
        total_tests=$(echo "$test_output" | grep -o "Total tests: [0-9]*" | grep -o "[0-9]*" || echo "0")
        passed_tests=$(echo "$test_output" | grep -o "Passed: [0-9]*" | grep -o "[0-9]*" || echo "0")
        failed_tests=$(echo "$test_output" | grep -o "Failed: [0-9]*" | grep -o "[0-9]*" || echo "0")
        skipped_tests=$(echo "$test_output" | grep -o "Skipped: [0-9]*" | grep -o "[0-9]*" || echo "0")
        
        log_result "PASS" "Tests executed successfully" "Total: $total_tests, Passed: $passed_tests, Failed: $failed_tests, Skipped: $skipped_tests"
        
        if [ "$failed_tests" -gt 0 ]; then
            log_result "WARN" "$failed_tests tests failed" "$project_name"
        fi
        
        return 0
    else
        log_result "FAIL" "Test execution failed" "$project_name"
        echo "$test_output" | tail -20 >> "$ERROR_LOG"
        return 1
    fi
}

# Function to analyze test coverage
analyze_test_coverage() {
    local swift_tests_dir=$1
    local csharp_tests_dir=$2
    
    echo "" | tee -a "$VERIFICATION_LOG"
    echo "=== TEST COVERAGE ANALYSIS ===" | tee -a "$VERIFICATION_LOG"
    
    # Count Swift test files
    local swift_test_count
    swift_test_count=$(find "$swift_tests_dir" -name "*Tests.swift" 2>/dev/null | wc -l || echo "0")
    
    # Count C# test files
    local csharp_test_count
    csharp_test_count=$(find "$csharp_tests_dir" -name "*Tests.cs" 2>/dev/null | wc -l || echo "0")
    
    log_result "INFO" "Swift test files found: $swift_test_count"
    log_result "INFO" "C# test files found: $csharp_test_count"
    
    if [ "$csharp_test_count" -ge "$swift_test_count" ]; then
        log_result "PASS" "Test file count verification" "C# tests ($csharp_test_count) >= Swift tests ($swift_test_count)"
    else
        local missing_count=$((swift_test_count - csharp_test_count))
        log_result "FAIL" "Missing test files" "$missing_count C# test files are missing"
    fi
    
    # List Swift test files that might not have C# equivalents
    echo "" | tee -a "$VERIFICATION_LOG"
    echo "=== DETAILED TEST FILE ANALYSIS ===" | tee -a "$VERIFICATION_LOG"
    
    if [ -d "$swift_tests_dir" ]; then
        while IFS= read -r swift_file; do
            local swift_basename
            swift_basename=$(basename "$swift_file" .swift)
            local csharp_equivalent="$csharp_tests_dir/$swift_basename.cs"
            
            if [ -f "$csharp_equivalent" ]; then
                log_result "PASS" "Test file converted: $swift_basename"
            else
                log_result "FAIL" "Missing C# test file: $swift_basename" "$csharp_equivalent not found"
            fi
        done < <(find "$swift_tests_dir" -name "*Tests.swift" 2>/dev/null || true)
    fi
}

# Function to check API compatibility
check_api_compatibility() {
    local csharp_tests_dir=$1
    
    echo "" | tee -a "$VERIFICATION_LOG"
    echo "=== API COMPATIBILITY CHECK ===" | tee -a "$VERIFICATION_LOG"
    
    # Check for common Swift to C# conversion issues
    local issues_found=0
    
    # Check for unconverted Swift patterns
    if grep -r "XCTAssert" "$csharp_tests_dir" > /dev/null 2>&1; then
        log_result "FAIL" "Swift XCTAssert patterns found" "Should be FluentAssertions"
        issues_found=$((issues_found + 1))
    else
        log_result "PASS" "No Swift XCTAssert patterns found"
    fi
    
    # Check for proper using statements
    if grep -r "using Xunit;" "$csharp_tests_dir" > /dev/null 2>&1; then
        log_result "PASS" "Xunit using statements found"
    else
        log_result "WARN" "No Xunit using statements found" "May indicate missing test framework"
    fi
    
    if grep -r "using FluentAssertions;" "$csharp_tests_dir" > /dev/null 2>&1; then
        log_result "PASS" "FluentAssertions using statements found"
    else
        log_result "WARN" "No FluentAssertions using statements found" "May indicate incomplete conversion"
    fi
    
    # Check for Swift-specific method calls that weren't converted
    local swift_patterns=("\.HexToByteArray\(\)" "XCTAssertEqual" "XCTAssertTrue" "XCTAssertFalse" "XCTAssertNotNil" "XCTAssertThrows")
    
    for pattern in "${swift_patterns[@]}"; do
        if grep -r "$pattern" "$csharp_tests_dir" > /dev/null 2>&1; then
            log_result "FAIL" "Unconverted Swift pattern found: $pattern"
            issues_found=$((issues_found + 1))
        fi
    done
    
    if [ $issues_found -eq 0 ]; then
        log_result "PASS" "No Swift conversion issues detected"
    fi
    
    # Check for proper C# patterns
    if grep -r "\.Should\(\)\." "$csharp_tests_dir" > /dev/null 2>&1; then
        log_result "PASS" "FluentAssertions patterns found"
    else
        log_result "WARN" "No FluentAssertions patterns found" "Tests may not be using proper assertion style"
    fi
}

# Function to check project structure
check_project_structure() {
    local base_dir=$1
    
    echo "" | tee -a "$VERIFICATION_LOG"
    echo "=== PROJECT STRUCTURE VERIFICATION ===" | tee -a "$VERIFICATION_LOG"
    
    # Check main project structure
    check_directory "$base_dir/src" "Source code directory"
    check_directory "$base_dir/tests" "Tests directory"
    check_directory "$base_dir/src/NeoSharp" "Main NeoSharp project"
    check_directory "$base_dir/tests/NeoSharp.Tests" "Main test project"
    
    # Check for project files
    if [ -f "$base_dir/src/NeoSharp/NeoSharp.csproj" ]; then
        log_result "PASS" "Main project file exists" "NeoSharp.csproj"
    else
        log_result "FAIL" "Main project file missing" "NeoSharp.csproj"
    fi
    
    if [ -f "$base_dir/tests/NeoSharp.Tests/NeoSharp.Tests.csproj" ]; then
        log_result "PASS" "Test project file exists" "NeoSharp.Tests.csproj"
    else
        log_result "FAIL" "Test project file missing" "NeoSharp.Tests.csproj"
    fi
    
    # Check solution file
    if [ -f "$base_dir/NeoSharp.sln" ]; then
        log_result "PASS" "Solution file exists" "NeoSharp.sln"
    else
        log_result "WARN" "Solution file missing" "NeoSharp.sln"
    fi
}

# Function to generate detailed report
generate_report() {
    echo "" | tee -a "$VERIFICATION_LOG"
    echo "=== VERIFICATION SUMMARY ===" | tee -a "$VERIFICATION_LOG"
    echo "Total Checks: $TOTAL_CHECKS" | tee -a "$VERIFICATION_LOG"
    echo "Passed: $PASSED_CHECKS" | tee -a "$VERIFICATION_LOG"
    echo "Failed: $FAILED_CHECKS" | tee -a "$VERIFICATION_LOG"
    echo "Success Rate: $(( (PASSED_CHECKS * 100) / TOTAL_CHECKS ))%" | tee -a "$VERIFICATION_LOG"
    echo "" | tee -a "$VERIFICATION_LOG"
    
    if [ $FAILED_CHECKS -eq 0 ]; then
        echo -e "${GREEN}🎉 CONVERSION VERIFICATION PASSED!${NC}" | tee -a "$VERIFICATION_LOG"
        echo "All checks passed successfully. The Neo Swift to Neo Sharp SDK conversion appears to be complete." | tee -a "$VERIFICATION_LOG"
    else
        echo -e "${RED}❌ CONVERSION VERIFICATION FAILED!${NC}" | tee -a "$VERIFICATION_LOG"
        echo "Found $FAILED_CHECKS issues that need to be addressed." | tee -a "$VERIFICATION_LOG"
        echo "Check $ERROR_LOG for detailed error information." | tee -a "$VERIFICATION_LOG"
    fi
    
    echo "" | tee -a "$VERIFICATION_LOG"
    echo "Completed: $(date)" | tee -a "$VERIFICATION_LOG"
}

# Main execution
main() {
    local base_dir="${1:-$(pwd)}"
    
    echo "Starting verification from directory: $base_dir"
    cd "$base_dir"
    
    # 1. Check project structure
    check_project_structure "$base_dir"
    
    # 2. Check build status
    if [ -d "$base_dir/src/NeoSharp" ]; then
        check_build "$base_dir" "NeoSharp Solution"
    fi
    
    # 3. Analyze test coverage (assuming Swift tests are in a reference directory)
    local swift_ref_dir="${base_dir}/../NeoSwift-Reference/Tests" # Adjust path as needed
    if [ -d "$swift_ref_dir" ]; then
        analyze_test_coverage "$swift_ref_dir" "$base_dir/tests/NeoSharp.Tests"
    else
        log_result "WARN" "Swift reference directory not found" "Cannot compare test coverage"
    fi
    
    # 4. Check API compatibility
    if [ -d "$base_dir/tests/NeoSharp.Tests" ]; then
        check_api_compatibility "$base_dir/tests/NeoSharp.Tests"
    fi
    
    # 5. Run tests if build succeeded
    if [ -d "$base_dir/tests/NeoSharp.Tests" ] && [ $FAILED_CHECKS -eq 0 ]; then
        run_tests "$base_dir" "NeoSharp.Tests"
    elif [ $FAILED_CHECKS -gt 0 ]; then
        log_result "WARN" "Skipping test execution due to build failures"
    fi
    
    # 6. Generate final report
    generate_report
    
    # Return appropriate exit code
    if [ $FAILED_CHECKS -eq 0 ]; then
        exit 0
    else
        exit 1
    fi
}

# Run main function with provided arguments
main "$@"