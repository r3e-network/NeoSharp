# Neo Swift to Neo Sharp SDK Conversion Verification Script (PowerShell)
# This script verifies that all Swift SDK unit tests have been properly converted to C# Sharp SDK

param(
    [string]$BasePath = (Get-Location).Path,
    [switch]$Verbose = $false
)

# Initialize counters and logs
$script:TotalChecks = 0
$script:PassedChecks = 0
$script:FailedChecks = 0

$VerificationLog = Join-Path $BasePath "verification-report.log"
$ErrorLog = Join-Path $BasePath "conversion-errors.log"

# Clear previous logs
"" | Out-File $VerificationLog
"" | Out-File $ErrorLog

function Write-Result {
    param(
        [string]$Status,
        [string]$Message,
        [string]$Details = ""
    )
    
    $script:TotalChecks++
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    
    switch ($Status) {
        "PASS" {
            $output = "✅ PASS: $Message"
            Write-Host $output -ForegroundColor Green
            $script:PassedChecks++
        }
        "FAIL" {
            $output = "❌ FAIL: $Message"
            Write-Host $output -ForegroundColor Red
            if ($Details) {
                $detailOutput = "   Details: $Details"
                Write-Host $detailOutput -ForegroundColor Red
                "$Message`: $Details" | Add-Content $ErrorLog
            }
            $script:FailedChecks++
        }
        "WARN" {
            $output = "⚠️  WARN: $Message"
            Write-Host $output -ForegroundColor Yellow
            if ($Details) {
                Write-Host "   Details: $Details" -ForegroundColor Yellow
            }
        }
        "INFO" {
            $output = "ℹ️  INFO: $Message"
            Write-Host $output -ForegroundColor Blue
        }
    }
    
    $logEntry = "[$timestamp] $output"
    if ($Details) {
        $logEntry += " - $Details"
    }
    $logEntry | Add-Content $VerificationLog
}

function Test-ProjectStructure {
    param([string]$BasePath)
    
    Write-Host "`n=== PROJECT STRUCTURE VERIFICATION ===" -ForegroundColor Cyan
    
    # Check directories
    $requiredDirs = @(
        @{Path = "src"; Description = "Source code directory"},
        @{Path = "tests"; Description = "Tests directory"},
        @{Path = "src\NeoSharp"; Description = "Main NeoSharp project"},
        @{Path = "tests\NeoSharp.Tests"; Description = "Main test project"}
    )
    
    foreach ($dir in $requiredDirs) {
        $fullPath = Join-Path $BasePath $dir.Path
        if (Test-Path $fullPath -PathType Container) {
            Write-Result "PASS" "Directory exists: $($dir.Description)" $fullPath
        } else {
            Write-Result "FAIL" "Directory missing: $($dir.Description)" $fullPath
        }
    }
    
    # Check project files
    $projectFiles = @(
        @{Path = "src\NeoSharp\NeoSharp.csproj"; Description = "Main project file"},
        @{Path = "tests\NeoSharp.Tests\NeoSharp.Tests.csproj"; Description = "Test project file"}
    )
    
    foreach ($file in $projectFiles) {
        $fullPath = Join-Path $BasePath $file.Path
        if (Test-Path $fullPath -PathType Leaf) {
            Write-Result "PASS" "Project file exists: $($file.Description)" $fullPath
        } else {
            Write-Result "FAIL" "Project file missing: $($file.Description)" $fullPath
        }
    }
    
    # Check solution file
    $solutionPath = Join-Path $BasePath "NeoSharp.sln"
    if (Test-Path $solutionPath -PathType Leaf) {
        Write-Result "PASS" "Solution file exists" $solutionPath
    } else {
        Write-Result "WARN" "Solution file missing" $solutionPath
    }
}

function Test-BuildStatus {
    param([string]$BasePath)
    
    Write-Host "`n=== BUILD VERIFICATION ===" -ForegroundColor Cyan
    
    Push-Location $BasePath
    
    try {
        # Clean build
        $cleanResult = dotnet clean 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Result "PASS" "Project clean successful"
        } else {
            Write-Result "FAIL" "Project clean failed" $cleanResult
            return $false
        }
        
        # Build project
        $buildResult = dotnet build 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Result "PASS" "Project builds successfully"
            
            # Check for warnings
            $warnings = $buildResult | Select-String "warning" 
            if ($warnings.Count -gt 0) {
                Write-Result "WARN" "Build has $($warnings.Count) warnings"
            }
            return $true
        } else {
            $errors = $buildResult | Select-String "error"
            Write-Result "FAIL" "Project build failed with $($errors.Count) errors"
            
            # Log first 10 errors
            $errors | Select-Object -First 10 | Add-Content $ErrorLog
            return $false
        }
    }
    finally {
        Pop-Location
    }
}

function Test-ApiCompatibility {
    param([string]$TestsPath)
    
    Write-Host "`n=== API COMPATIBILITY CHECK ===" -ForegroundColor Cyan
    
    if (-not (Test-Path $TestsPath)) {
        Write-Result "FAIL" "Tests directory not found" $TestsPath
        return
    }
    
    $issuesFound = 0
    
    # Check for unconverted Swift patterns
    $swiftPatterns = @("XCTAssert", "\.HexToByteArray\(\)", "XCTAssertEqual", "XCTAssertTrue", "XCTAssertFalse")
    
    foreach ($pattern in $swiftPatterns) {
        $matches = Get-ChildItem $TestsPath -Recurse -Filter "*.cs" | Select-String $pattern
        if ($matches.Count -gt 0) {
            Write-Result "FAIL" "Unconverted Swift pattern found: $pattern" "$($matches.Count) occurrences"
            $issuesFound++
        }
    }
    
    if ($issuesFound -eq 0) {
        Write-Result "PASS" "No Swift conversion issues detected"
    }
    
    # Check for proper C# patterns
    $csharpFiles = Get-ChildItem $TestsPath -Recurse -Filter "*.cs"
    
    $xunitUsing = $csharpFiles | Select-String "using Xunit;"
    if ($xunitUsing.Count -gt 0) {
        Write-Result "PASS" "Xunit using statements found"
    } else {
        Write-Result "WARN" "No Xunit using statements found"
    }
    
    $fluentUsing = $csharpFiles | Select-String "using FluentAssertions;"
    if ($fluentUsing.Count -gt 0) {
        Write-Result "PASS" "FluentAssertions using statements found"
    } else {
        Write-Result "WARN" "No FluentAssertions using statements found"
    }
    
    $fluentPatterns = $csharpFiles | Select-String "\.Should\(\)\."
    if ($fluentPatterns.Count -gt 0) {
        Write-Result "PASS" "FluentAssertions patterns found"
    } else {
        Write-Result "WARN" "No FluentAssertions patterns found"
    }
}

function Test-TestExecution {
    param([string]$BasePath)
    
    Write-Host "`n=== TEST EXECUTION ===" -ForegroundColor Cyan
    
    Push-Location $BasePath
    
    try {
        $testResult = dotnet test --verbosity minimal 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            # Extract test statistics
            $totalTests = ($testResult | Select-String "Total tests: (\d+)" | ForEach-Object { $_.Matches.Groups[1].Value }) -as [int]
            $passedTests = ($testResult | Select-String "Passed: (\d+)" | ForEach-Object { $_.Matches.Groups[1].Value }) -as [int]
            $failedTests = ($testResult | Select-String "Failed: (\d+)" | ForEach-Object { $_.Matches.Groups[1].Value }) -as [int]
            $skippedTests = ($testResult | Select-String "Skipped: (\d+)" | ForEach-Object { $_.Matches.Groups[1].Value }) -as [int]
            
            $stats = "Total: $totalTests, Passed: $passedTests, Failed: $failedTests, Skipped: $skippedTests"
            Write-Result "PASS" "Tests executed successfully" $stats
            
            if ($failedTests -gt 0) {
                Write-Result "WARN" "$failedTests tests failed"
            }
            
            return $true
        } else {
            Write-Result "FAIL" "Test execution failed"
            $testResult | Select-Object -Last 20 | Add-Content $ErrorLog
            return $false
        }
    }
    finally {
        Pop-Location
    }
}

function Test-TestCoverage {
    param([string]$CSharpTestsPath, [string]$SwiftTestsPath = $null)
    
    Write-Host "`n=== TEST COVERAGE ANALYSIS ===" -ForegroundColor Cyan
    
    # Count C# test files
    $csharpTestFiles = Get-ChildItem $CSharpTestsPath -Recurse -Filter "*Tests.cs" -ErrorAction SilentlyContinue
    $csharpCount = $csharpTestFiles.Count
    
    Write-Result "INFO" "C# test files found: $csharpCount"
    
    if ($SwiftTestsPath -and (Test-Path $SwiftTestsPath)) {
        $swiftTestFiles = Get-ChildItem $SwiftTestsPath -Recurse -Filter "*Tests.swift" -ErrorAction SilentlyContinue
        $swiftCount = $swiftTestFiles.Count
        
        Write-Result "INFO" "Swift test files found: $swiftCount"
        
        if ($csharpCount -ge $swiftCount) {
            Write-Result "PASS" "Test file count verification" "C# tests ($csharpCount) >= Swift tests ($swiftCount)"
        } else {
            $missingCount = $swiftCount - $csharpCount
            Write-Result "FAIL" "Missing test files" "$missingCount C# test files are missing"
        }
        
        # Check individual file conversions
        Write-Host "`n=== DETAILED TEST FILE ANALYSIS ===" -ForegroundColor Cyan
        
        foreach ($swiftFile in $swiftTestFiles) {
            $baseName = [System.IO.Path]::GetFileNameWithoutExtension($swiftFile.Name)
            $csharpEquivalent = Join-Path $CSharpTestsPath ($baseName + ".cs")
            
            if (Test-Path $csharpEquivalent) {
                Write-Result "PASS" "Test file converted: $baseName"
            } else {
                Write-Result "FAIL" "Missing C# test file: $baseName" $csharpEquivalent
            }
        }
    } else {
        Write-Result "WARN" "Swift reference directory not found" "Cannot compare test coverage"
    }
}

function Write-FinalReport {
    Write-Host "`n=== VERIFICATION SUMMARY ===" -ForegroundColor Cyan
    
    $successRate = if ($script:TotalChecks -gt 0) { [math]::Round(($script:PassedChecks * 100) / $script:TotalChecks, 1) } else { 0 }
    
    Write-Host "Total Checks: $script:TotalChecks" -ForegroundColor White
    Write-Host "Passed: $script:PassedChecks" -ForegroundColor Green
    Write-Host "Failed: $script:FailedChecks" -ForegroundColor Red
    Write-Host "Success Rate: $successRate%" -ForegroundColor White
    
    $summary = @"

=== VERIFICATION SUMMARY ===
Total Checks: $script:TotalChecks
Passed: $script:PassedChecks
Failed: $script:FailedChecks
Success Rate: $successRate%

"@
    
    $summary | Add-Content $VerificationLog
    
    if ($script:FailedChecks -eq 0) {
        Write-Host "`n🎉 CONVERSION VERIFICATION PASSED!" -ForegroundColor Green
        Write-Host "All checks passed successfully. The Neo Swift to Neo Sharp SDK conversion appears to be complete." -ForegroundColor Green
    } else {
        Write-Host "`n❌ CONVERSION VERIFICATION FAILED!" -ForegroundColor Red
        Write-Host "Found $script:FailedChecks issues that need to be addressed." -ForegroundColor Red
        Write-Host "Check $ErrorLog for detailed error information." -ForegroundColor Red
    }
    
    Write-Host "`nCompleted: $(Get-Date)" -ForegroundColor White
    "Completed: $(Get-Date)" | Add-Content $VerificationLog
}

# Main execution
function Main {
    Write-Host "=== Neo Swift to Neo Sharp SDK Conversion Verification ===" -ForegroundColor Cyan
    Write-Host "Started: $(Get-Date)" -ForegroundColor White
    Write-Host "Base Path: $BasePath" -ForegroundColor White
    
    "=== Neo Swift to Neo Sharp SDK Conversion Verification ===" | Add-Content $VerificationLog
    "Started: $(Get-Date)" | Add-Content $VerificationLog
    "Base Path: $BasePath" | Add-Content $VerificationLog
    "" | Add-Content $VerificationLog
    
    # 1. Check project structure
    Test-ProjectStructure $BasePath
    
    # 2. Check build status
    $buildSuccess = Test-BuildStatus $BasePath
    
    # 3. Check API compatibility
    $testsPath = Join-Path $BasePath "tests\NeoSharp.Tests"
    if (Test-Path $testsPath) {
        Test-ApiCompatibility $testsPath
    }
    
    # 4. Analyze test coverage
    $swiftRefPath = Join-Path (Split-Path $BasePath -Parent) "NeoSwift-Reference\Tests"
    Test-TestCoverage $testsPath $swiftRefPath
    
    # 5. Run tests if build succeeded
    if ($buildSuccess -and ($script:FailedChecks -eq 0)) {
        Test-TestExecution $BasePath
    } elseif ($script:FailedChecks -gt 0) {
        Write-Result "WARN" "Skipping test execution due to build failures"
    }
    
    # 6. Generate final report
    Write-FinalReport
    
    # Return appropriate exit code
    if ($script:FailedChecks -eq 0) {
        exit 0
    } else {
        exit 1
    }
}

# Run main function
Main