namespace EventsChecker.Core

open System

type SOReputationChecker(userName, userId) =
    inherit SimpleValueCheckerBase<int>("sorep-"+userName) //
    
    override this.HasValueChanged oldv newv =
        oldv <> newv.ToString()
        
    override this.ConvertValueToStore value =
        value.ToString()
        
    override this.GetNewestValue() =
        let url = sprintf "http://stackoverflow.com/users/flair/%d.json" userId
        let json = JsonDownloader.downloadJson url
        json.["reputation"] |> JsonDownloader.parseJsonInt
        
    override this.NotifyChange() =
        [sprintf "Changed SO Reputation for user %s(%d). Value: %d" userName userId this.ChangedValue.Value]
        
    override this.ToString() =
        sprintf "SOReputationChecker - %s(%d)" userName userId

    override this.GetName() =
        userName