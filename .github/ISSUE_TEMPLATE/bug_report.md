---
name: Bug Report
about: Create a report to help us improve
title: '[BUG] '
labels: ['bug']
assignees: ''
---

## Bug Description

<!-- A clear and concise description of what the bug is -->

## Steps to Reproduce

<!-- Steps to reproduce the behavior -->

1. 
2. 
3. 
4. 

## Expected Behavior

<!-- A clear and concise description of what you expected to happen -->

## Actual Behavior

<!-- A clear and concise description of what actually happened -->

## Environment

<!-- Please complete the following information -->

- **OS**: [e.g. Windows 11, Ubuntu 22.04, macOS 13.0]
- **.NET Version**: [e.g. .NET 9.0]
- **PingdomExporter Version**: [e.g. v1.2.3]
- **Installation Method**: [e.g. GitHub Release, built from source]

## Configuration

<!-- Please provide relevant configuration (remove sensitive information like API tokens) -->

```json
{
  "BaseUrl": "https://api.pingdom.com/api/3.1/",
  "OutputDirectory": "exports",
  "OutputFormat": "json",
  "ExportMode": "Summary"
}
```

**Command Line Arguments Used:**
```bash
dotnet run -- --your-arguments-here
```

## Error Output

<!-- If applicable, add the complete error message or stack trace -->

```
Paste error output here
```

## Log Output

<!-- If using verbose mode, please include relevant log output -->

```
Paste log output here (use --verbose flag)
```

## Screenshots

<!-- If applicable, add screenshots to help explain your problem -->

## Additional Context

<!-- Add any other context about the problem here -->

### API-Related Issues
<!-- If this is related to Pingdom API -->

- [ ] This issue occurs with specific checks only
- [ ] This issue occurs with all checks
- [ ] This issue is related to authentication
- [ ] This issue is related to rate limiting

**Check IDs affected (if applicable):** 

### Export-Related Issues
<!-- If this is related to export functionality -->

- [ ] Issue occurs with JSON export
- [ ] Issue occurs with CSV export
- [ ] Issue occurs with UptimeRobot format
- [ ] Issue occurs with file creation/writing

### Workaround

<!-- If you found a workaround, please describe it -->

## Checklist

<!-- Please check the boxes that apply -->

- [ ] I have searched existing issues to ensure this is not a duplicate
- [ ] I have provided all the requested information
- [ ] I have removed sensitive information (API tokens, etc.)
- [ ] I can reproduce this issue consistently
- [ ] I have tested with the latest version
