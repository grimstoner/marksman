/*
 * Copyright 2019 marksman Contributors (https://github.com/Scope-IT/marksman)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections.Generic;

namespace Marksman.Types
{
    public class WindowsSystemTypes : PCSystemTypes
    {
        public WindowsSystemTypes()
        {
            this.SystemTypes = new Dictionary<string, string>();
            this.SystemTypes.Add("0", "Undefined");
            this.SystemTypes.Add("1", "Desktop");
            this.SystemTypes.Add("2", "Laptop");
            this.SystemTypes.Add("3", "Workstation");
            this.SystemTypes.Add("4", "Enterprise Server");
            this.SystemTypes.Add("5", "SOHO Server");
            this.SystemTypes.Add("6", "Appliance PC");
            this.SystemTypes.Add("7", "Performance Server");
            this.SystemTypes.Add("8", "Maximum");
        }
    }
}
