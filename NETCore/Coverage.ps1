$artifactLocation = 'artifacts\bin\Nito.CalculatedProperties\Debug\net45'
iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/StephenCleary/BuildTools/master/Coverage.ps1'))
